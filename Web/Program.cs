using WhatTheWord.Api.Hubs;
using WhatTheWord.Api.Configuration;
using WhatTheWord.Domain.Services;

namespace WhatTheWord.Api;

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
        var environment = builder.Environment;

        services.AddControllersWithViews();

        services.AddCorsPolicy(environment);

        services.AddHostedService<GameCoordinator>();

        // add mediatr service
        services.AddMediatorService();

        // add database services
        services.AddCosmosDbService(configuration);
        services.AddRepositoryServices();

        // signalR services
        services.AddSignalR();

        // game service
        services.AddGameService(configuration);

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
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCorsPolicy(app.Environment);

        app.MapControllers();
        app.MapHub<GameHub>("/game");
        app.MapFallbackToFile("index.html");

        return app;
    }
}
