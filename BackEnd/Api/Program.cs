using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using OhMyWord.Api.Extensions;
using OhMyWord.Api.Hubs;
using OhMyWord.Api.Services;
using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Options;
using OhMyWord.Infrastructure.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        var appBuilder = WebApplication.CreateBuilder(args);

        // azure app configuration
        var appConfigEnabled = appBuilder.Configuration.GetValue("AppConfig:Enabled", true);
        if (appConfigEnabled)
            appBuilder.Configuration.AddAzureAppConfiguration(options =>
            {
                var endpoint = appBuilder.Configuration.GetValue<string>("AppConfig:Endpoint") ??
                               throw new InvalidOperationException("Application configuration endpoint is not set.");
                var appEnv = appBuilder.Configuration.GetValue<string>("AppConfig:Environment", "dev");
                options.Connect(new Uri(endpoint), new DefaultAzureCredential())
                    .Select(KeyFilter.Any)
                    .Select(KeyFilter.Any, "shared")
                    .Select(KeyFilter.Any, appEnv)
                    .ConfigureKeyVault(vaultOptions => vaultOptions.SetCredential(new DefaultAzureCredential()));
            });

        // configure app host
        appBuilder.Host.ConfigureServices((context, services) =>
        {
            // microsoft identity authentication services
            services.AddMicrosoftIdentityAuthentication(context);

            // fast endpoints
            services.AddFastEndpoints();

            // signalR services
            services.AddSignalR()
                .AddJsonProtocol(options =>
                    options.PayloadSerializerOptions.Converters.Add(
                        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase))
                );

            // game services
            services.AddHostedService<GameCoordinator>();
            services.AddSingleton<IGameService, GameService>();
            services.AddOptions<GameServiceOptions>()
                .Bind(context.Configuration.GetSection(GameServiceOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // local project services
            services.AddDomainServices();
            services.AddCosmosDbRepositories(context);
            services.AddTableRepositories(context);
            services.AddMessagingServices(context);
            services.AddRapidApiServices();

            // development services
            if (context.HostingEnvironment.IsDevelopment())
            {
                services.AddCors();
            }

            // health checks
            services.AddApplicationHealthChecks(context.Configuration);
        });

        // build the application
        var app = appBuilder.Build();

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

        // run the application
        app.Run();
    }
}
