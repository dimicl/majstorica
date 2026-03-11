using backend.Domain.Exceptions;

namespace backend.Domain.Entities;


/*Kada firma zaposli majstora, napravimo membership zapis:
var member = new CompanyMember(
    Guid.NewGuid(),
    companyId,
    userId,
    DateTime.UtcNow);

I tada u servisu promeniš user-a:
user.PromoteMasterToCompanyWorker();

Kada majstor napusti firmu
member.Deactivate(DateTime.UtcNow);
user.ReturnCompanyWorkerToMaster();*/
public class CompanyMember
{
    private CompanyMember()
    {
        // Potrebno za serializer / mapper / Mongo
    }

    public CompanyMember(
        Guid id,
        Guid companyId,
        Guid userId,
        DateTime joinedAtUtc)
    {
        if (id == Guid.Empty)
            throw new DomainException("CompanyMember id cannot be empty.");

        if (companyId == Guid.Empty)
            throw new DomainException("CompanyId cannot be empty.");

        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");

        Id = id;
        CompanyId = companyId;
        UserId = userId;

        IsActive = true;
        JoinedAtUtc = joinedAtUtc;
        UpdatedAtUtc = joinedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid CompanyId { get; private set; }

    public Guid UserId { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime JoinedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public DateTime? LeftAtUtc { get; private set; }

    public void Deactivate(DateTime leftAtUtc)
    {
        if (!IsActive)
            return;

        IsActive = false;
        LeftAtUtc = leftAtUtc;

        Touch();
    }

    public void Reactivate()
    {
        if (IsActive)
            return;

        IsActive = true;
        LeftAtUtc = null;

        Touch();
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}