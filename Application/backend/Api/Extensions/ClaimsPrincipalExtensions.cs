using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend.Domain.Enums;
using backend.Shared.Exceptions;

namespace backend.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var (userId, _) = GetUserIdAndRoleCore(user);
        return userId;
    }

    public static (Guid userId, UserRole role) GetUserIdAndRole(this ClaimsPrincipal user)
    {
        return GetUserIdAndRoleCore(user);
    }

    private static (Guid userId, UserRole role) GetUserIdAndRoleCore(ClaimsPrincipal user)
    {
        var userIdStr =
            user.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            user.FindFirstValue(ClaimTypes.NameIdentifier) ??
            user.FindFirstValue(ClaimTypes.Name) ??
            user.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var guid))
            throw new UnauthorizedException("Nedostaje ili je neispravan identifikator korisnika u tokenu.");

        var roleStr = user.FindFirstValue(ClaimTypes.Role) ?? nameof(UserRole.Client);
        if (!Enum.TryParse<UserRole>(roleStr, ignoreCase: true, out var role))
            role = UserRole.Client;

        return (guid, role);
    }
}

