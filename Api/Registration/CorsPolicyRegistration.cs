namespace OhMyWord.Api.Registration;

public static class CorsPolicyRegistration
{

    /// <summary>
    /// Use CORS policy when running in development environment.
    /// </summary>
    public static void UseLocalCorsPolicy(this WebApplication app)
    {
        app.UseCors(builder => builder.WithOrigins("http://localhost:4200")
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod()
        );
    }
}
