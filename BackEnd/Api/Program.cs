using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using OhMyWord.Api.Hubs;
using OhMyWord.Api.Registration;
using OhMyWord.Core.Mapping;
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
            .ConfigureServices((context, collection) =>
            {
                // microsoft identity authentication services
                collection.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApi(context.Configuration);

                // fast endpoints
                collection.AddFastEndpoints();

                // add database services
                collection.AddCosmosDbService(context.Configuration);
                collection.AddRepositoryServices();

                // signalR services
                collection.AddSignalR()
                    .AddJsonProtocol(options =>
                        options.PayloadSerializerOptions.Converters.Add(
                            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase))
                    );

                // game services
                collection.AddGameServices(context.Configuration);

                // automapper service
                collection.AddAutoMapper(typeof(EntitiesProfile));

                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                collection.AddEndpointsApiExplorer();
                collection.AddSwaggerGen();

                // health checks        
                collection.AddHealthChecks()
                    .AddCosmosDbCollection(
                        context.Configuration.GetValue<string>("CosmosDb:ConnectionString") ?? string.Empty,
                        context.Configuration.GetValue<string>("CosmosDb:DatabaseId"),
                        context.Configuration.GetValue<string[]>("CosmosDb:ContainerIds"));
            });

        // build the application
        var app = appBuilder.Build();

        app.UseAuthorization();
        app.UseFastEndpoints(config =>
        {
            config.Endpoints.RoutePrefix = "api";
        });
        app.MapHub<GameHub>("/hub");
        app.UseHealthChecks("/health");

        // run the application
        app.Run();
    }
}
