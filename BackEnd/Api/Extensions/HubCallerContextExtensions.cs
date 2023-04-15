using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using System.Net;
using System.Security.Claims;

namespace OhMyWord.Api.Extensions;

public static class HubCallerContextExtensions
{
    public static Guid? GetUserId(this HubCallerContext context)
        => Guid.TryParse(context.User?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value,
            out var userId)
            ? userId
            : null;

    public static IPAddress GetIpAddress(this HubCallerContext context)
        => context.Features.GetRequiredFeature<IHttpConnectionFeature>().RemoteIpAddress ?? IPAddress.None;
}
