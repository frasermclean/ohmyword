using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.FeatureManagement;
using OhMyWord.Api.Services;
using OhMyWord.Domain.DependencyInjection;
using OhMyWord.Integrations.CosmosDb.DependencyInjection;
using OhMyWord.Integrations.GraphApi.DependencyInjection;
using OhMyWord.Integrations.RapidApi.DependencyInjection;
using OhMyWord.Integrations.ServiceBus.DependencyInjection;
using OhMyWord.Integrations.Storage.DependencyInjection;
using Serilog;

namespace OhMyWord.Api.Startup;

public static class HostConfiguration
{
    /// <summary>
    ///     Adds all application services to the <see cref="WebApplicationBuilder" />
    /// </summary>
    public static WebApplication ConfigureAndBuildHost(this WebApplicationBuilder builder)
    {
        builder.Host
            .UseSerilog(ConfigureSerilog)
            .ConfigureServices(AddServices);

        return builder.Build();
    }

    private static void ConfigureSerilog(HostBuilderContext context, IServiceProvider serviceProvider,
        LoggerConfiguration configuration)
    {
        var telemetryConfiguration = serviceProvider.GetRequiredService<TelemetryConfiguration>();
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces);
    }

    private static void AddServices(HostBuilderContext context, IServiceCollection collection)
    {
        collection
            .AddApplicationInsightsTelemetry()
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
            .AddServiceBusServices(context.Configuration)
            .AddRapidApiServices()
            .AddGraphApiClient(context.Configuration);

        // configure JSON serialization
        collection.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });

        // development services
        if (context.HostingEnvironment.IsDevelopment()) collection.AddCors();
    }
}