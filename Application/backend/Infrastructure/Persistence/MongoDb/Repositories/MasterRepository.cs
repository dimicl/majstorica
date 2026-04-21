using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Infrastructure.Persistence.MongoDb;

public class MasterRepository : IMasterRepository
{
    private readonly IUserRepository _userRepository;

    public MasterRepository(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Save(Guid userId, MasterProfile masterProfile)
    {
        var user = await _userRepository.GetById(userId)
            ?? throw new InvalidOperationException($"User {userId} not found. Cannot save master profile.");

        user.SetMasterProfile(masterProfile);
        await _userRepository.Save(user);
    }

    public async Task<MasterProfile?> GetByUserId(Guid userId)
    {
        var user = await _userRepository.GetById(userId);
        if (user == null)
            return null;

        if ((user.Role == UserRole.Master /* || user.Role == UserRole.CompanyWorker */) &&
            user.MasterProfile == null)
        {
            user.SetMasterProfile(MasterProfile.CreateDefaultShell());
            await _userRepository.Save(user);
        }

        return user.MasterProfile;
    }

    public async Task<IReadOnlyDictionary<Guid, MasterProfile?>> GetByUserIds(IEnumerable<Guid> userIds)
    {
        var idList = userIds.Distinct().ToList();
        if (idList.Count == 0)
            return new Dictionary<Guid, MasterProfile?>();

        var users = await _userRepository.GetByIds(idList);
        return users.ToDictionary(u => u.Id, u => u.MasterProfile);
    }
}
