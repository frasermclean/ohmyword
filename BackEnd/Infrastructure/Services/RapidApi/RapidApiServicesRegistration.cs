using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OhMyWord.Infrastructure.Options;

namespace OhMyWord.Infrastructure.Services.RapidApi;

public static class RapidApiServicesRegistration
{
    public static IServiceCollection AddRapidApiServices(this IServiceCollection services, HostBuilderContext context)
    {
        services.AddOptions<RapidApiOptions>()
            .BindConfiguration(RapidApiOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var apiKey = context.Configuration["RapidApi:ApiKey"] ?? string.Empty;

        return services;
    }
}
