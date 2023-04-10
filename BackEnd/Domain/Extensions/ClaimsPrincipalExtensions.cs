using System.Security.Claims;

namespace OhMyWord.Domain.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Get the user ID from the claims principal.
    /// </summary>
    /// <param name="principal">Claims principal to inspect.</param>
    /// <returns>User Id has a <see cref="Guid"/> if it could be detected, null if not.</returns>
    public static Guid? GetUserId(this ClaimsPrincipal principal)
        => Guid.TryParse(principal.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value,
            out var userId)
            ? userId
            : default;
}
