using backend.Domain.Enums;
using backend.Domain.Exceptions;
using backend.Domain.ValueObjects;
using backend.Shared.Helpers;

namespace backend.Domain.Entities;

public class User
{
    private User()
    {
        // Potrebno za serializer / mapper / Mongo
    }

    public User(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string passwordHash,
        UserRole role,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
            throw new DomainException("User id cannot be empty.");

        ValidateInitialRole(role);

        Id = id;
        SetFirstName(firstName);
        SetLastName(lastName);
        SetEmail(email);
        SetPhoneNumber(phoneNumber);
        SetPasswordHash(passwordHash);

        Role = role;
        IsActive = true;
        IsBlocked = false;

        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();

    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; }
    public bool IsBlocked { get; private set; }

    public Address? Address { get; private set; }
    public GeoLocation? GeoLocation { get; private set; }

    public ClientProfile? ClientProfile { get; private set; }
    public MasterProfile? MasterProfile { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }
    public DateTime? BlockedAtUtc { get; private set; }

    public bool IsClient() => Role == UserRole.Client;
    public bool IsMaster() => Role == UserRole.Master;
    public bool IsCompanyOwner() => Role == UserRole.CompanyOwner;
    public bool IsCompanyWorker() => Role == UserRole.CompanyWorker;
    public bool IsAdmin() => Role == UserRole.Admin;

    public void UpdateBasicInfo(
        string firstName,
        string lastName,
        string phoneNumber)
    {
        EnsureNotBlocked();

        SetFirstName(firstName);
        SetLastName(lastName);
        SetPhoneNumber(phoneNumber);

        Touch();
    }

    public void ChangeEmail(string email)
    {
        EnsureNotBlocked();
        SetEmail(email);
        Touch();
    }

    public void ChangePassword(string passwordHash)
    {
        EnsureNotBlocked();
        SetPasswordHash(passwordHash);
        Touch();
    }

    public void SetAddress(Address? address)
    {
        EnsureNotBlocked();
        Address = address;
        Touch();
    }

    public void SetGeoLocation(GeoLocation? geoLocation)
    {
        EnsureNotBlocked();
        GeoLocation = geoLocation;
        Touch();
    }

    public void SetClientProfile(ClientProfile clientProfile)
    {
        EnsureNotBlocked();

        if (Role != UserRole.Client)
            throw new DomainException("Only users with Client role can have client profile.");

        ClientProfile = clientProfile ?? throw new DomainException("Client profile cannot be null.");
        Touch();
    }

    public void SetMasterProfile(MasterProfile masterProfile)
    {
        EnsureNotBlocked();

        if (Role != UserRole.Master && Role != UserRole.CompanyWorker)
            throw new DomainException("Only Master or CompanyWorker can have master profile.");

        MasterProfile = masterProfile ?? throw new DomainException("Master profile cannot be null.");
        Touch();
    }

    public void RemoveClientProfile()
    {
        EnsureNotBlocked();

        if (Role != UserRole.Client)
            throw new DomainException("Only users with Client role can remove client profile.");

        ClientProfile = null;
        Touch();
    }

    public void RemoveMasterProfile()
    {
        EnsureNotBlocked();

        if (Role != UserRole.Master && Role != UserRole.CompanyWorker)
            throw new DomainException("Only Master or CompanyWorker can remove master profile.");

        MasterProfile = null;
        Touch();
    }

    public void PromoteMasterToCompanyWorker()
    {
        EnsureNotBlocked();

        if (Role != UserRole.Master)
            throw new DomainException("Only user with Master role can become CompanyWorker.");

        if (MasterProfile is null)
            throw new DomainException("Master profile is required before becoming CompanyWorker.");

        Role = UserRole.CompanyWorker;
        Touch();
    }

    public void ReturnCompanyWorkerToMaster()
    {
        EnsureNotBlocked();

        if (Role != UserRole.CompanyWorker)
            throw new DomainException("Only user with CompanyWorker role can return to Master.");

        if (MasterProfile is null)
            throw new DomainException("Master profile must exist when returning to Master.");

        Role = UserRole.Master;
        Touch();
    }

    public void Activate()
    {
        if (IsBlocked)
            throw new DomainException("Blocked user cannot be activated.");

        if (IsActive)
            return;

        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        Touch();
    }

    public void Block()
    {
        if (IsBlocked)
            return;

        IsBlocked = true;
        IsActive = false;
        BlockedAtUtc = DateTime.UtcNow;

        Touch();
    }

    public void Unblock()
    {
        if (!IsBlocked)
            return;

        IsBlocked = false;
        IsActive = true;
        BlockedAtUtc = null;

        Touch();
    }

    public void RecordLogin(DateTime loginAtUtc)
    {
        LastLoginAtUtc = loginAtUtc;
        Touch();
    }

    private void ValidateInitialRole(UserRole role)
    {
        if (role != UserRole.Client &&
            role != UserRole.Master &&
            role != UserRole.CompanyOwner)
        {
            throw new DomainException(
                "At registration, user role must be Client, Master, or CompanyOwner.");
        }
    }

    private void SetFirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");

        FirstName = firstName.Trim();
    }

    private void SetLastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");

        LastName = lastName.Trim();
    }

    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");

        Email = email.Trim().ToLowerInvariant();
    }

    private void SetPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException("Phone number is required.");

        PhoneNumber = phoneNumber.Trim();
    }

    private void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        PasswordHash = passwordHash;
    }

    private void EnsureNotBlocked()
    {
        if (IsBlocked)
            throw new DomainException("Blocked user cannot be modified.");
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}