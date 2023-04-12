using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace OhMyWord.Api.Extensions;

public static class MicrosoftIdentityRegistration
{
    private const string SectionName = "AzureAd";

    public static IServiceCollection AddMicrosoftIdentityAuthentication(this IServiceCollection services,
        HostBuilderContext context)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(options =>
            {
                context.Configuration.GetSection(SectionName).Bind(options);
                options.Events = new JwtBearerEvents { OnMessageReceived = OnMessageReceived };
            }, options =>
            {
                context.Configuration.GetSection(SectionName).Bind(options);
            });

        return services;
    }

    private static Task OnMessageReceived(MessageReceivedContext receivedContext)
    {
        if (receivedContext.Request.Path.Value != "/hub") return Task.CompletedTask;

        // read the access token from the query string
        var accessToken = receivedContext.Request.Query["access_token"];
        if (!accessToken.Any()) return Task.CompletedTask;

        receivedContext.Token = accessToken;
        return Task.CompletedTask;
    }
}
