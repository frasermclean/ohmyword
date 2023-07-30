using Microsoft.FeatureManagement;
using OhMyWord.Api.Services;
using OhMyWord.Domain.DependencyInjection;
using OhMyWord.Infrastructure.DependencyInjection;
using Serilog;

namespace OhMyWord.Api.Startup;

public static class ServicesRegistration
{
    /// <summary>
    /// Adds all application services to the <see cref="WebApplicationBuilder"/>
    /// </summary>    
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, provider, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(provider);
        });
        
        builder.Host.ConfigureServices((context, collection) =>
        {
            // microsoft identity authentication services
            collection.AddMicrosoftIdentityAuthentication(context);

            // feature management
            collection.AddFeatureManagement();

            // fast endpoints
            collection.AddFastEndpoints();

            // signalR services
            collection.AddSignalRServices(context);

            // game services
            collection.AddHostedService<GameBackgroundService>();

            // local project services
            collection.AddDomainServices(context.Configuration)
                .AddCosmosDbRepositories(context)
                .AddTableRepositories(context.Configuration)
                .AddMessagingServices(context)
                .AddRapidApiServices()
                .AddGraphApiClient(context.Configuration);

            // development services
            if (context.HostingEnvironment.IsDevelopment())
            {
                collection.AddCors();
            }

            // health checks
            collection.AddApplicationHealthChecks(context.Configuration);
        });

        return builder;
    }
}
