namespace WhatTheWord.Api.Services;

public static class CorsPolicy
{
    public const string PolicyName = "LocalDevelopment";

    /// <summary>
    /// Add CORS policy for use in development.
    /// </summary>
    public static void AddCorsPolicy(this IServiceCollection services, IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
            return;

        services.AddCors(options => options.AddPolicy(PolicyName, builder =>
        {
            builder
                .WithOrigins("http://localhost:4200")
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }));
    }

    /// <summary>
    /// Use CORS policy when running in development environment.
    /// </summary>
    public static void UseCorsPolicy(this WebApplication app, IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
            return;

        app.UseCors(PolicyName);
    }
}
