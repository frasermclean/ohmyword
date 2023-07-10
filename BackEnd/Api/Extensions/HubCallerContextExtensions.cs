using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using System.Net;
using System.Security.Claims;

namespace OhMyWord.Api.Extensions;

public static class HubCallerContextExtensions
{
    private static readonly string[] ForwardHeaders = { "X-Forwarded-For", "X-Real-IP" };

    public static Guid? GetUserId(this HubCallerContext context)
        => Guid.TryParse(context.User?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value,
            out var userId)
            ? userId
            : null;

    public static IPAddress GetIpAddress(this HubCallerContext context)
    {
        var httpContext = context.GetHttpContext();

        if (httpContext is null)
            return IPAddress.None;

        foreach (var header in ForwardHeaders)
        {
            if (httpContext.Request.Headers.TryGetValue(header, out var addresses) &&
                IPAddress.TryParse(addresses.FirstOrDefault(), out var address))
            {
                return address;
            }                
        }
        
        return IPAddress.None;
    }
}
