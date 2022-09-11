using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace OhMyWord.Api.Registration;

public static class MicrosoftIdentityRegistration
{
    public static void AddMicrosoftIdentity(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(jwtBearerOptions =>
            {                
                jwtBearerOptions.TokenValidationParameters.NameClaimType = "name";
            }, microsoftIdentityOptions =>
            {
                microsoftIdentityOptions.Instance = "https://ohmywordb2c.b2clogin.com";
                microsoftIdentityOptions.Domain = "ohmywordb2c.onmicrosoft.com";
                microsoftIdentityOptions.ClientId = "da1cf4ec-9558-4f92-a8d3-f3c7ec0f5fa2";
                microsoftIdentityOptions.TenantId = "670c3284-1150-41e2-b323-9297ac9e5f53";
                microsoftIdentityOptions.SignUpSignInPolicyId = "B2C_1A_SignUp_SignIn";
            });
    }
}
