using backend.Domain.Entities;

namespace backend.Application.Helpers;

internal static class UserDisplayNameHelper
{
    public static string GetDisplayName(User? user, string fallback = "Korisnik")
    {
        if (user == null) return fallback;
        var name = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrEmpty(name) ? fallback : name;
    }
}
