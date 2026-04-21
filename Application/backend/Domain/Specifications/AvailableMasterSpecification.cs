using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Domain.Specifications;

public class AvailableMasterSpecification : ISpecification<User>
{
    public bool IsSatisfiedBy(User user)
    {
        if (user is null)
            return false;

        var isMasterRole = user.Role == UserRole.Master /* || user.Role == UserRole.CompanyWorker */;

        return user.IsActive
               && !user.IsBlocked
               && isMasterRole
               && user.MasterProfile is not null
               && user.MasterProfile.IsAvailable;
    }
}