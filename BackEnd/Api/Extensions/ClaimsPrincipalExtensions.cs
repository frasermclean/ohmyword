using System.Security.Claims;

namespace OhMyWord.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Attempt to extract the user ID from the claims principal.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> to operate on.</param>
    /// <returns>The user ID in <see cref="Guid"/> format.</returns>
    public static Guid GetUserId(this ClaimsPrincipal principal)
        => Guid.TryParse(principal.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value,
            out var userId)
            ? userId
            : Guid.Empty;
}
