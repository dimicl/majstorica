using backend.Domain.Enums;

namespace backend.Application.Interfaces;

public interface IAuthService
{
    Task<string> Register(
        string firstName,
        string lastName,
        string email,
        string username,
        string password,
        UserRole role);

    Task<string> Login(string usernameOrEmail, string password);
}
