using Azure.Identity;
using FluentValidation;
using MediatR;
using Microsoft.Identity.Web;
using OhMyWord.Api.Hubs;
using OhMyWord.Api.Registration;
using OhMyWord.Core.Handlers.Words;
using System.Text.Json;
using System.Text.Json.Serialization;
using OhMyWord.Core.Mapping;
using OhMyWord.Core.Validators.Words;

namespace OhMyWord.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        var appBuilder = WebApplication.CreateBuilder(args);
        var configuration = appBuilder.Configuration;

        // configure app host
        appBuilder.Host
            .ConfigureAppConfiguration(builder =>
            {
                // add azure app configuration store via managed identity
                var endpoint = configuration.GetValue<string>("AppConfigEndpoint");
                if (string.IsNullOrEmpty(endpoint)) return;
                builder.AddAzureAppConfiguration(options =>
                {
                    var uri = new Uri(endpoint);
                    options.Connect(uri, new ManagedIdentityCredential());
                });
            })
            .ConfigureServices(services => AddServices(services, configuration));

        // build the app and configure the request pipeline
        var app = appBuilder.Build().ConfigureRequestPipeline();

        // run the application
        app.Run();
    }

    /// <summary>
    /// Add application services to the service collection.
    /// </summary>
    private static void AddServices(IServiceCollection services, IConfigurationRoot configuration)
    {
        // set up routing options
        services.AddRouting(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
        });

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });

        // add microsoft identity authentication services
        services.AddMicrosoftIdentityWebApiAuthentication(configuration);

        // add database services
        services.AddCosmosDbService(configuration);
        services.AddRepositoryServices();

        // signalR services
        services.AddSignalR()
            .AddJsonProtocol(options =>
                options.PayloadSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase))
            );

        // game services
        services.AddGameServices(configuration);

        // add fluent validation validators
        services.AddValidatorsFromAssemblyContaining<CreateWordRequestValidator>();

        // add mediatr service
        services.AddMediatR(typeof(CreateWordHandler));

        // automapper service
        services.AddAutoMapper(typeof(EntitiesProfile));

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    private static WebApplication ConfigureRequestPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseLocalCorsPolicy();
            app.UseDeveloperExceptionPage();
        }

        // enable serving static content
        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<GameHub>("/hub");

        // fall back to SPA index file on unhandled route
        app.UseEndpoints(configure => configure.MapFallbackToFile("/index.html"));

        return app;
    }
}
