using System.Security.Claims;

namespace backend.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userId =
            user.FindFirstValue(ClaimTypes.NameIdentifier) ??
            user.FindFirstValue(ClaimTypes.Name) ??
            user.FindFirstValue("sub");

        return Guid.Parse(userId!);
    }
}

