using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OhMyWord.Infrastructure.Options;

namespace OhMyWord.Infrastructure.DependencyInjection;

public static class GraphApiClientRegistration
{
    public static IServiceCollection AddGraphApiClient(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<GraphApiClientOptions>()
            .Bind(configuration.GetSection(GraphApiClientOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
