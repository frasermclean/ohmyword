using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Serilog;

namespace OhMyWord.Api.Startup;

public static class AzureAppConfiguration
{
    /// <summary>
    /// Configures Azure App Configuration for the application
    /// </summary>    
    public static WebApplicationBuilder AddAzureAppConfiguration(this WebApplicationBuilder builder)
    {
        var endpoint = builder.Configuration["APP_CONFIG_ENDPOINT"];
        if (string.IsNullOrEmpty(endpoint))
        {
            Log.Information("Azure App Configuration endpoint is not set, using local configuration");
            return builder;
        }

        Log.Information("Using Azure App Configuration with endpoint: {Endpoint} and environment: {AppEnv}",
            endpoint, builder.Environment.EnvironmentName);

        builder.Configuration.AddAzureAppConfiguration(options =>
        {
            options.Connect(new Uri(endpoint), new DefaultAzureCredential())
                .Select(KeyFilter.Any)
                .Select(KeyFilter.Any, builder.Environment.EnvironmentName)
                .ConfigureKeyVault(vaultOptions => vaultOptions.SetCredential(new DefaultAzureCredential()))
                .UseFeatureFlags(flagOptions =>
                {
                    flagOptions.Select(KeyFilter.Any)
                        .Select(KeyFilter.Any, builder.Environment.EnvironmentName);
                    flagOptions.CacheExpirationInterval = TimeSpan.FromMinutes(5);
                });
        });

        return builder;
    }
}
