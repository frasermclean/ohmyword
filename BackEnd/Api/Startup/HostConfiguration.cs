using Microsoft.FeatureManagement;
using OhMyWord.Api.Services;
using OhMyWord.Domain.DependencyInjection;
using OhMyWord.Integrations.DependencyInjection;
using Serilog;

namespace OhMyWord.Api.Startup;

public static class HostConfiguration
{
    /// <summary>
    /// Adds all application services to the <see cref="WebApplicationBuilder"/>
    /// </summary>    
    public static WebApplication ConfigureAndBuildHost(this WebApplicationBuilder builder)
    {
        builder.Host
            .UseSerilog(ConfigureSerilog)
            .ConfigureServices(AddServices);

        return builder.Build();
    }

    private static void ConfigureSerilog(HostBuilderContext context, IServiceProvider provider,
        LoggerConfiguration configuration)
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(provider);
    }

    private static void AddServices(HostBuilderContext context, IServiceCollection collection)
    {
        collection
            .AddMicrosoftIdentityAuthentication(context.Configuration)
            .AddSignalRServices(context.Configuration)
            .AddFastEndpoints()
            .AddApplicationHealthChecks(context.Configuration)
            .AddFeatureManagement();

        // local project services
        collection
            .AddHostedService<GameBackgroundService>()
            .AddDomainServices(context.Configuration)
            .AddCosmosDbRepositories(context.Configuration)
            .AddTableRepositories(context.Configuration)
            .AddMessagingServices(context.Configuration)
            .AddRapidApiServices()
            .AddGraphApiClient(context.Configuration);

        // development services
        if (context.HostingEnvironment.IsDevelopment())
        {
            collection.AddCors();
        }
    }
}
