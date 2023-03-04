using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using OhMyWord.Api.Hubs;
using OhMyWord.Api.Options;
using OhMyWord.Api.Services;
using OhMyWord.Data.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        var appBuilder = WebApplication.CreateBuilder(args);

        // configure app host
        appBuilder.Host
            .ConfigureServices((context, services) =>
            {
                // microsoft identity authentication services
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApi(context.Configuration);

                // fast endpoints
                services.AddFastEndpoints();

                // add data services
                services.AddDataServices(context.Configuration);

                // signalR services
                services.AddSignalR()
                    .AddJsonProtocol(options =>
                        options.PayloadSerializerOptions.Converters.Add(
                            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase))
                    );

                // game services
                services.Configure<GameServiceOptions>(context.Configuration.GetSection("Game"));
                services.AddHostedService<GameCoordinator>();
                services.AddSingleton<IGameService, GameService>();
                services.AddSingleton<IVisitorService, VisitorService>();
                services.AddSingleton<IWordsService, WordsService>();

                // development services
                if (context.HostingEnvironment.IsDevelopment())
                {
                    services.AddCors();
                }

                // health checks        
                services.AddHealthChecks()
                    .AddCosmosDbCollection(
                        context.Configuration.GetValue<string>("CosmosDb:ConnectionString") ?? string.Empty,
                        context.Configuration.GetValue<string>("CosmosDb:DatabaseId"),
                        context.Configuration.GetValue<string[]>("CosmosDb:ContainerIds"));
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
