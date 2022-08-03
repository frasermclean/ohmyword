using Azure.Identity;
using FluentValidation;
using MediatR;
using Microsoft.Identity.Web;
using OhMyWord.Api.Hubs;
using OhMyWord.Api.Registration;
using OhMyWord.Core.Behaviours;
using OhMyWord.Core.Handlers.Words;
using OhMyWord.Core.Mapping;
using OhMyWord.Core.Validators.Words;
using Serilog;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Api;

public static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            var appBuilder = WebApplication.CreateBuilder(args);

            // configure app host
            appBuilder.Host
                .ConfigureAppConfiguration((context, builder) =>
                    AddAzureAppConfiguration(builder, context.Configuration))
                .UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration))
                .ConfigureServices((context, collection) => collection.AddServices(context.Configuration));

            // build the app and configure the request pipeline
            var app = appBuilder.Build().ConfigureRequestPipeline();

            // run the application
            Log.Information("Starting web application");
            app.Run();

            return 0;
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Add Azure application configuration service.
    /// </summary>   
    private static void AddAzureAppConfiguration(IConfigurationBuilder builder, IConfiguration configuration)
    {
        var disabled = configuration.GetValue<bool>("AppConfig:Disabled");
        if (disabled) return; // do not attempt to use AzureAppConfiguration

        var connectionString = configuration.GetValue<string>("AppConfig:ConnectionString");
        var endpoint = configuration.GetValue<string>("AppConfig:Endpoint");

        builder.AddAzureAppConfiguration(options =>
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                // connect using connection string
                options.Connect(connectionString);
                return;
            }

            if (string.IsNullOrEmpty(endpoint)) return;

            // attempt connect using default azure credential 
            var uri = new Uri(endpoint);
            options.Connect(uri, new DefaultAzureCredential());
        });
    }

    /// <summary>
    /// Add application services to the service collection.
    /// </summary>
    private static void AddServices(this IServiceCollection services, IConfiguration configuration)
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
        services.AddMediatR(typeof(CreateWordHandler))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        // automapper service
        services.AddAutoMapper(typeof(EntitiesProfile));

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // health checks        
        services.AddHealthChecks()
            .AddCosmosDbCollection(configuration.GetValue<string>("CosmosDb:ConnectionString"),
                configuration.GetValue<string>("CosmosDb:DatabaseId"),
                configuration.GetValue<string[]>("CosmosDb:ContainerIds"));
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

        app.UseHealthChecks("/api/health");

        // fall back to SPA index file on unhandled route
        app.UseEndpoints(routeBuilder => routeBuilder.MapFallbackToFile("/index.html"));

        return app;
    }
}
