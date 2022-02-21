using WhatTheWord.Domain.Services;

namespace WhatTheWord.Api.Services;

public static class GameService
{
    public static IServiceCollection AddGameService(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddOptions<GameServiceOptions>()
            .Bind(configuration.GetSection("Game"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IGameService, Domain.Services.GameService>();

        return services;
    }
}
