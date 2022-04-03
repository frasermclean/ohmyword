using System.Text.Json;
using System.Text.Json.Serialization;
using OhMyWord.Api.Hubs;
using OhMyWord.Api.Registration;
using OhMyWord.Api.Responses;

namespace OhMyWord.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        // build configured application
        var app = WebApplication.CreateBuilder(args)
            .AddServices()
            .Build()
            .ConfigureRequestPipeline();

        // run the application
        app.Run();
    }

    private static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });

        // add database services
        services.AddCosmosDbService(configuration);
        services.AddRepositoryServices();

        // signalR services
        services.AddSignalR();

        // game services
        services.AddGameServices(configuration);

        // object mapping service
        services.AddAutoMapper(typeof(MappingProfile));

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return builder;
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
        
        app.UseRouting();

        app.MapControllers();
        app.MapHub<GameHub>("/game");

        return app;
    }
}
