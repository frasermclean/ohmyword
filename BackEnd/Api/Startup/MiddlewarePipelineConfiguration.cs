using OhMyWord.Api.Hubs;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Api.Startup;

public static class MiddlewarePipelineConfiguration
{
    /// <summary>
    /// Configures the application middleware pipeline.
    /// </summary>
    /// <param name="app">The application to configure.</param>
    /// <returns>The <see cref="WebApplication"/></returns>
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
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
                var isAuthorizationEnabled = app.Configuration.GetValue("FeatureManagement:Authorization", true);
                if (isAuthorizationEnabled)
                {
                    endpoint.Roles("admin");
                }
                else
                {
                    endpoint.AllowAnonymous();
                }
            };
            config.Serializer.Options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });

        app.MapHub<GameHub>("/hub");
        app.UseHealthChecks("/health");

        return app;
    }
}
