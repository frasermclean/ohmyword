using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using OhMyWord.Api.Hubs;
using OhMyWord.Api.Registration;
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
                services.AddGameServices(context.Configuration);

                // development services
                if (context.HostingEnvironment.IsDevelopment())
                {
                    services.AddCors();
                }

                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen();

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
            config.Serializer.Options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });

        app.MapHub<GameHub>("/hub");
        app.UseHealthChecks("/health");

        // run the application
        app.Run();
    }
}
