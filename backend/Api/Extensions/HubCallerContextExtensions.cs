using Microsoft.AspNetCore.SignalR;
using System.Net;

namespace OhMyWord.Api.Extensions;

public static class HubCallerContextExtensions
{
    private static readonly string[] ForwardHeaders = { "X-Forwarded-For", "X-Real-IP" };

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
