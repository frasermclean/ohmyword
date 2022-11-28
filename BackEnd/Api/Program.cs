using FluentValidation;
using MediatR;
using OhMyWord.Api.Hubs;
using OhMyWord.Api.Registration;
using OhMyWord.Core.Behaviours;
using OhMyWord.Core.Handlers.Words;
using OhMyWord.Core.Mapping;
using OhMyWord.Core.Validators.Words;
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
                // set up routing options
                collection.AddRouting(options =>
                {
                    options.LowercaseUrls = true;
                    options.LowercaseQueryStrings = true;
                });

                collection.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.Converters.Add(
                            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                    });

                // microsoft identity authentication services
                collection.AddMicrosoftIdentity();

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

                // add fluent validation validators
                collection.AddValidatorsFromAssemblyContaining<CreateWordRequestValidator>();

                // add mediatr service
                collection.AddMediatR(typeof(CreateWordHandler))
                    .AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>))
                    .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

                // automapper service
                collection.AddAutoMapper(typeof(EntitiesProfile));

                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                collection.AddEndpointsApiExplorer();
                collection.AddSwaggerGen();

                // health checks        
                collection.AddHealthChecks()
                    .AddCosmosDbCollection(
                        context.Configuration.GetValue<string>("CosmosDb:ConnectionString"),
                        context.Configuration.GetValue<string>("CosmosDb:DatabaseId"),
                        context.Configuration.GetValue<string[]>("CosmosDb:ContainerIds"));
            });

        // build the application
        var app = appBuilder.Build();

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

        // run the application
        app.Run();
    }
}
