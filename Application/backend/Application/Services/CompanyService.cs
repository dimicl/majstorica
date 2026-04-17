using backend.Api.DTOs.Company;
using backend.Application.Helpers;
using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.Exceptions;
using backend.Domain.ValueObjects;
using backend.Shared.Exceptions;

namespace backend.Application.Services;

public class CompanyService : ICompanyService
{
    private static readonly TimeSpan CompanyPublicCacheTtl = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan InviteSearchCacheTtl = TimeSpan.FromMinutes(3);

    private readonly ICompanyRepository _companies;
    private readonly ICompanyInvitationRepository _invitations;
    private readonly IUserRepository _users;
    private readonly IMasterRepository _masters;
    private readonly ICompanyInvitationRealtimeSender _realtime;
    private readonly IUserGraphSync _userGraphSync;
    private readonly IRedisJsonCache _redisJsonCache;

    public CompanyService(
        ICompanyRepository companies,
        ICompanyInvitationRepository invitations,
        IUserRepository users,
        IMasterRepository masters,
        ICompanyInvitationRealtimeSender realtime,
        IUserGraphSync userGraphSync,
        IRedisJsonCache redisJsonCache)
    {
        _companies = companies;
        _invitations = invitations;
        _users = users;
        _masters = masters;
        _realtime = realtime;
        _userGraphSync = userGraphSync;
        _redisJsonCache = redisJsonCache;
    }

    public async Task<CompanyResponse?> GetMineForOwner(Guid ownerUserId)
    {
        var company = await _companies.GetByOwnerUserId(ownerUserId);
        return company is null ? null : BuildCompanyResponse(company);
    }

    public async Task<CompanyPublicResponse?> GetPublicById(Guid companyId)
    {
        var cacheKey = CompanyPublicCacheKey.ForCompany(companyId);
        try
        {
            var cached = await _redisJsonCache.GetAsync<CompanyPublicResponse>(cacheKey);
            if (cached != null)
                return cached;
        }
        catch
        {
        }

        var company = await _companies.GetById(companyId);
        if (company is null || !company.IsActive)
            return null;

        var dto = new CompanyPublicResponse
        {
            Id = company.Id,
            OwnerUserId = company.OwnerUserId,
            Name = company.Name,
            Description = company.Description,
            PhoneNumber = company.PhoneNumber,
            Email = company.Email,
            City = company.Address?.City,
            ServiceCategories = company.ServiceCategories.ToList()
        };

        try
        {
            await _redisJsonCache.SetAsync(cacheKey, dto, CompanyPublicCacheTtl);
        }
        catch
        {
        }

        return dto;
    }

    public async Task<CompanyResponse> CreateForOwner(
        Guid ownerUserId,
        string name,
        string phoneNumber,
        string email,
        string? street,
        string? city)
    {
        if (await _companies.GetByOwnerUserId(ownerUserId) != null)
            throw new ConflictException("Firma za ovog korisnika već postoji.");

        Address? address = null;
        var hasStreet = !string.IsNullOrWhiteSpace(street);
        var hasCity = !string.IsNullOrWhiteSpace(city);
        if (hasStreet || hasCity)
        {
            if (!hasStreet || !hasCity)
                throw new DomainException(
                    "Za adresu firme unesi i ulicu i grad, ili ostavi oba polja prazna.");

            address = new Address(street!.Trim(), city!.Trim());
        }

        var company = new Company(
            Guid.NewGuid(),
            name.Trim(),
            ownerUserId,
            null,
            phoneNumber.Trim(),
            email.Trim().ToLowerInvariant(),
            address,
            null,
            null,
            DateTime.UtcNow);

        await _companies.Save(company);
        return BuildCompanyResponse(company);
    }

    public async Task<List<MasterSearchForInviteResponse>> SearchMastersForInvite(
        Guid ownerUserId,
        string? query,
        int limit)
    {
        _ = await RequireCompanyForOwner(ownerUserId);
        var q = query ?? string.Empty;
        var searchKey = CompanyInviteSearchCacheKey.Create(ownerUserId, q, limit);
        try
        {
            var cached = await _redisJsonCache.GetAsync<List<MasterSearchForInviteResponse>>(searchKey);
            if (cached != null)
                return cached;
        }
        catch
        {
        }

        var list = await _users.SearchMastersForCompanyInvite(q, limit, ownerUserId);
        if (list.Count == 0)
            return new List<MasterSearchForInviteResponse>();

        var ids = list.Select(u => u.Id).ToList();
        var profiles = await _masters.GetByUserIds(ids);

        var result = list.Select(u =>
        {
            profiles.TryGetValue(u.Id, out var profile);
            var headline = profile?.Headline;
            return new MasterSearchForInviteResponse
            {
                UserId = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Username = u.Username,
                Headline = string.IsNullOrWhiteSpace(headline) ? null : headline
            };
        }).ToList();

        try
        {
            await _redisJsonCache.SetAsync(searchKey, result, InviteSearchCacheTtl);
        }
        catch
        {
        }

        return result;
    }

    public async Task InviteMaster(Guid ownerUserId, Guid masterUserId)
    {
        if (masterUserId == Guid.Empty)
            throw new DomainException("Majstor nije izabran.");

        var company = await RequireCompanyForOwner(ownerUserId);
        if (masterUserId == ownerUserId)
            throw new DomainException("Ne možeš pozvati samog sebe.");

        var master = await _users.GetById(masterUserId);
        if (master is null)
            throw new NotFoundException("Korisnik nije pronađen.");

        EnsureMasterEligibleForInvite(master);

        var existing = await _invitations.GetPendingByCompanyAndMaster(company.Id, masterUserId);
        if (existing != null)
            throw new ConflictException("Pozivnica ovom majstoru je već na čekanju.");

        var invitation = CompanyInvitation.CreatePending(company.Id, masterUserId);
        await _invitations.Save(invitation);

        await _realtime.SendInvitationAsync(
            masterUserId,
            invitation.Id,
            company.Id,
            company.Name);
    }

    public async Task<List<Guid>> GetPendingOutboundInviteMasterIdsForOwner(Guid ownerUserId)
    {
        var company = await RequireCompanyForOwner(ownerUserId);
        var pending = await _invitations.GetPendingForCompany(company.Id);
        return pending.Select(p => p.MasterUserId).Distinct().ToList();
    }

    public async Task<List<CompanyInvitationPendingResponse>> GetPendingInvitationsForMaster(
        Guid masterUserId)
    {
        var pending = await _invitations.GetPendingForMaster(masterUserId);
        var result = new List<CompanyInvitationPendingResponse>();
        foreach (var inv in pending)
        {
            var company = await _companies.GetById(inv.CompanyId);
            if (company is null)
                continue;

            result.Add(new CompanyInvitationPendingResponse
            {
                InvitationId = inv.Id,
                CompanyId = inv.CompanyId,
                CompanyName = company.Name,
                CreatedAtUtc = inv.CreatedAtUtc
            });
        }

        return result;
    }

    public async Task AcceptInvitation(Guid masterUserId, Guid invitationId)
    {
        var invitation = await _invitations.GetById(invitationId);
        if (invitation is null)
            throw new NotFoundException("Pozivnica nije pronađena.");

        if (invitation.MasterUserId != masterUserId)
            throw new ForbiddenException("Ova pozivnica nije za tebe.");

        if (invitation.Status != CompanyInvitationStatus.Pending)
            throw new ConflictException("Pozivnica više nije aktivna.");

        var company = await _companies.GetById(invitation.CompanyId);
        if (company is null || !company.IsActive)
            throw new ConflictException("Firma više nije dostupna.");

        var master = await _users.GetById(masterUserId);
        if (master is null)
            throw new NotFoundException("Korisnik nije pronađen.");

        EnsureMasterEligibleForInvite(master);

        master.PromoteMasterToCompanyWorker(company.Id);
        await _users.Save(master);
        await _userGraphSync.SyncUserNode(masterUserId, UserRole.CompanyWorker);

        invitation.MarkAccepted();
        await _invitations.Save(invitation);
    }

    public async Task DeclineInvitation(Guid masterUserId, Guid invitationId)
    {
        var invitation = await _invitations.GetById(invitationId);
        if (invitation is null)
            throw new NotFoundException("Pozivnica nije pronađena.");

        if (invitation.MasterUserId != masterUserId)
            throw new ForbiddenException("Ova pozivnica nije za tebe.");

        if (invitation.Status != CompanyInvitationStatus.Pending)
            throw new ConflictException("Pozivnica više nije aktivna.");

        invitation.MarkDeclined();
        await _invitations.Save(invitation);
    }

    public async Task<List<CompanyWorkerMemberResponse>> GetWorkersForCompanyOwner(Guid ownerUserId)
    {
        var company = await RequireCompanyForOwner(ownerUserId);
        var users = await _users.GetWorkersForCompany(company.Id);
        if (users.Count == 0)
            return new List<CompanyWorkerMemberResponse>();

        var ids = users.Select(u => u.Id).ToList();
        var profiles = await _masters.GetByUserIds(ids);

        return users.Select(u =>
        {
            profiles.TryGetValue(u.Id, out var profile);
            var headline = profile?.Headline;
            return new CompanyWorkerMemberResponse
            {
                UserId = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Username = u.Username,
                Email = u.Email,
                PhoneNumber = string.IsNullOrWhiteSpace(u.PhoneNumber) ? null : u.PhoneNumber,
                Headline = string.IsNullOrWhiteSpace(headline) ? null : headline,
                Description = string.IsNullOrWhiteSpace(profile?.Description) ? null : profile.Description,
                YearsOfExperience = profile?.YearsOfExperience ?? 0,
                HourlyRateAmount = profile?.HourlyRate?.Amount ?? 0,
                HourlyRateCurrency = profile?.HourlyRate?.Currency ?? "RSD",
                IsAvailable = profile?.IsAvailable ?? false,
                ServiceCategories = profile?.ServiceCategories.ToList() ?? new List<string>(),
                ServiceZones = profile?.ServiceZones.ToList() ?? new List<string>(),
                AverageRating = profile?.AverageRating?.Value,
                TotalJobsCompleted = profile?.TotalJobsCompleted ?? 0,
                TotalReviews = profile?.TotalReviews ?? 0
            };
        }).ToList();
    }

    private async Task<Company> RequireCompanyForOwner(Guid ownerUserId)
    {
        var company = await _companies.GetByOwnerUserId(ownerUserId);
        if (company is null)
            throw new NotFoundException("Nemaš registrovanu firmu.");
        if (!company.IsActive)
            throw new ConflictException("Firma nije aktivna.");
        return company;
    }

    private static void EnsureMasterEligibleForInvite(User master)
    {
        if (!master.IsActive || master.IsBlocked)
            throw new ConflictException("Ovaj nalog nije dostupan za poziv.");

        if (master.Role != UserRole.Master)
            throw new ConflictException("Možeš pozvati samo nezavisne majstore.");

        if (master.EmployerCompanyId.HasValue)
            throw new ConflictException("Majstor je već u nekoj firmi.");

        if (master.MasterProfile is null)
            throw new ConflictException("Majstor nema profil.");
    }

    private static CompanyResponse BuildCompanyResponse(Company company) =>
        new()
        {
            Id = company.Id,
            Name = company.Name,
            Description = company.Description,
            PhoneNumber = company.PhoneNumber,
            Email = company.Email,
            OwnerUserId = company.OwnerUserId,
        };
}
