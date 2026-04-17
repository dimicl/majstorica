using backend.Domain.Entities;
using backend.Domain.ValueObjects;
using backend.Infrastructure.Persistence.MongoDb.Entities;

namespace backend.Infrastructure.Persistence.MongoDb.Mappers;

public static class CompanyMapper
{
    public static CompanyDocument ToDocument(Company company)
    {
        return new CompanyDocument
        {
            Id = company.Id,
            OwnerUserId = company.OwnerUserId,
            Name = company.Name,
            Description = company.Description,
            PhoneNumber = company.PhoneNumber,
            Email = company.Email,
            AddressStreet = company.Address?.Street,
            AddressCity = company.Address?.City,
            ServiceCategories = company.ServiceCategories.ToList(),
            IsActive = company.IsActive,
            CreatedAtUtc = company.CreatedAtUtc,
            UpdatedAtUtc = company.UpdatedAtUtc,
        };
    }

    public static Company ToDomain(CompanyDocument doc)
    {
        Address? address = null;
        if (!string.IsNullOrWhiteSpace(doc.AddressStreet) &&
            !string.IsNullOrWhiteSpace(doc.AddressCity))
            address = new Address(doc.AddressStreet!.Trim(), doc.AddressCity!.Trim());

        var company = new Company(
            doc.Id,
            doc.Name,
            doc.OwnerUserId,
            doc.Description,
            doc.PhoneNumber,
            doc.Email,
            address,
            null,
            doc.ServiceCategories.Count > 0 ? doc.ServiceCategories.ToList() : null,
            doc.CreatedAtUtc);

        if (!doc.IsActive)
            company.Deactivate();

        return company;
    }
}
