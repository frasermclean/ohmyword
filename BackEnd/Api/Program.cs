using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;
using OhMyWord.Api.Extensions;
using OhMyWord.Api.Hubs;
using OhMyWord.Api.Services;
using OhMyWord.Domain.DependencyInjection;
using OhMyWord.Infrastructure.DependencyInjection;
using Serilog;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        // create serilog bootstrap logger
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting application");

            var builder = WebApplication.CreateBuilder(args);

            ConfigureAppConfiguration(builder);

            // configure app host
            builder.Host
                .UseSerilog((context, provider, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(provider))
                .ConfigureServices(AddServices);

            // build the application and configure the pipeline
            var app = builder.Build();
            ConfigurePipeline(app);

            // run the application
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureAppConfiguration(WebApplicationBuilder builder)
    {
        // azure app configuration
        var isAzureAppConfigEnabled = builder.Configuration.GetValue("AppConfig:Enabled", false);
        if (!isAzureAppConfigEnabled)
        {
            return;
        }

        Log.Information("Configuring Azure App Configuration");

        builder.Configuration.AddAzureAppConfiguration(options =>
        {
            var endpoint = builder.Configuration.GetValue<string>("AppConfig:Endpoint") ??
                           throw new InvalidOperationException("Application configuration endpoint is not set.");
            var appEnv = builder.Configuration.GetValue<string>("AppConfig:Environment", "dev");
            options.Connect(new Uri(endpoint), new DefaultAzureCredential())
                .Select(KeyFilter.Any)
                .Select(KeyFilter.Any, appEnv)
                .ConfigureKeyVault(vaultOptions => vaultOptions.SetCredential(new DefaultAzureCredential()))
                .UseFeatureFlags(flagOptions =>
                {
                    flagOptions.Select(KeyFilter.Any)
                        .Select(KeyFilter.Any, appEnv);
                    flagOptions.CacheExpirationInterval = TimeSpan.FromMinutes(5);
                });
        });
    }

    private static void AddServices(HostBuilderContext context, IServiceCollection collection)
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
    }

    private static void ConfigurePipeline(WebApplication app)
    {
        app.UseAuthorization();

        // development pipeline
        if (app.Environment.IsDevelopment())
        {
            // enable CORS policy
            app.UseCors(builder => builder.WithOrigins("http://localhost:4200")
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod());
        }

        app.UseFastEndpoints(config =>
        {
            config.Endpoints.RoutePrefix = "api";
            config.Endpoints.Configurator = endpoint =>
            {
                endpoint.Roles("admin");
            };
            config.Serializer.Options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });

        app.MapHub<GameHub>("/hub");
        app.UseHealthChecks("/health");
    }
}
