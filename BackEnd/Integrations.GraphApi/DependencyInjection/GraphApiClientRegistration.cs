using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using OhMyWord.Core.Services;
using OhMyWord.Integrations.GraphApi.Options;
using OhMyWord.Integrations.GraphApi.Services;

namespace OhMyWord.Integrations.GraphApi.DependencyInjection;

public static class GraphApiClientRegistration
{
    public static IServiceCollection AddGraphApiClient(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<GraphApiClientOptions>()
            .Bind(configuration.GetSection(GraphApiClientOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<GraphServiceClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<GraphApiClientOptions>>().Value;
            var credential = new ClientSecretCredential(options.TenantId, options.ClientId, options.ClientSecret);

            return new GraphServiceClient(credential);
        });

        services.AddSingleton<IGraphApiClient, GraphApiClient>();

        return services;
    }
}
