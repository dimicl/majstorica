using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> GetById(Guid userId)
    {
        return await _userRepository.GetById(userId);
    }

    public async Task UpdateProfile(
        Guid userId,
        string firstName,
        string lastName)
    {
        var user = await _userRepository.GetById(userId);
        if (user == null)
            throw new Exception("Korisnik nije pronađen.");

        user.UpdateProfile(firstName, lastName);

        await _userRepository.Save(user);
    }

    public async Task Deactivate(Guid userId)
    {
        var user = await _userRepository.GetById(userId);
        if (user == null)
            throw new Exception("Korisnik nije pronađen.");

        user.Deactivate();
        await _userRepository.Save(user);
    }

    public async Task Activate(Guid userId)
    {
        var user = await _userRepository.GetById(userId);
        if (user == null)
            throw new Exception("Korisnik nije pronađen.");

        user.Activate();
        await _userRepository.Save(user);
    }

    public async Task<List<User>> GetAllMasters()
    {
        var users = await _userRepository.GetAll();
        return users
            .Where(u => u.Role == UserRole.Master && u.IsActive)
            .ToList();
    }
}
