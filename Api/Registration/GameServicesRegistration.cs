using OhMyWord.Services.Game;
using OhMyWord.Services.Options;

namespace OhMyWord.Api.Registration;

public static class GameServicesRegistration
{
    public static void AddGameServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddOptions<GameServiceOptions>()
            .Bind(configuration.GetSection(GameServiceOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHostedService<GameCoordinator>();
        services.AddSingleton<IGameService, GameService>();
    }
}
