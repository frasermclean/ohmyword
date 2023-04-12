using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using OhMyWord.Api.Extensions;
using OhMyWord.Api.Hubs;
using OhMyWord.Api.Services;
using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Options;
using OhMyWord.Infrastructure.Extensions;
using OhMyWord.WordsApi.HealthChecks;
using OhMyWord.WordsApi.Services;
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
                    .AddMicrosoftIdentityWebApi(options =>
                    {
                        context.Configuration.GetSection("AzureAd").Bind(options);
                        options.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = receivedContext =>
                            {
                                if (receivedContext.Request.Path.Value != "/hub")
                                    return Task.CompletedTask;

                                // read the access token from the query string
                                var accessToken = receivedContext.Request.Query["access_token"];
                                if (!accessToken.Any())
                                    return Task.CompletedTask;

                                receivedContext.Token = accessToken;
                                return Task.CompletedTask;
                            }
                        };
                    }, options =>
                    {
                        context.Configuration.GetSection("AzureAd").Bind(options);
                    });


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
                services.AddUsersRepository(context);
                services.AddWordsApiClient(context);

                // development services
                if (context.HostingEnvironment.IsDevelopment())
                {
                    services.AddCors();
                }

                // health checks        
                services.AddApplicationHealthChecks(context);
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
