using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace OhMyWord.Api.Registration;

public static class MicrosoftIdentityRegistration
{
    public static void AddMicrosoftIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(jwtBearerOptions =>
            {
                configuration.Bind("AzureAd", jwtBearerOptions);
                jwtBearerOptions.TokenValidationParameters.NameClaimType = "name";
            }, microsoftIdentityOptions =>
            {
                configuration.Bind("AzureAd", microsoftIdentityOptions);
            });
    }
}
