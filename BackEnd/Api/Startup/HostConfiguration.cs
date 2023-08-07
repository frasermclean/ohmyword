using Microsoft.FeatureManagement;
using OhMyWord.Api.Services;
using OhMyWord.Domain.DependencyInjection;
using OhMyWord.Infrastructure.DependencyInjection;
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
        var configuration = context.Configuration;

        collection
            .AddMicrosoftIdentityAuthentication(configuration)
            .AddSignalRServices(configuration)
            .AddFastEndpoints()
            .AddApplicationHealthChecks(configuration)
            .AddFeatureManagement();

        // local project services
        collection
            .AddHostedService<GameBackgroundService>()
            .AddDomainServices(configuration)
            .AddCosmosDbRepositories(configuration)
            .AddTableRepositories(configuration)
            .AddMessagingServices(configuration)
            .AddRapidApiServices()
            .AddGraphApiClient(configuration);

        // development services
        if (context.HostingEnvironment.IsDevelopment())
        {
            collection.AddCors();
        }
    }
}
