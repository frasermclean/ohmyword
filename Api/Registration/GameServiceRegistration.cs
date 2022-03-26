using OhMyWord.Api.Services;

namespace OhMyWord.Api.Registration;

public static class GameServiceRegistration
{
    public static void AddGameService(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddOptions<GameServiceOptions>()
            .Bind(configuration.GetSection("Game"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IGameService, GameService>();
    }
}
