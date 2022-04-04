using OhMyWord.Api.Options;
using OhMyWord.Services.Game;

namespace OhMyWord.Api.Registration;

public static class GameServicesRegistration
{
    public static void AddGameServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddOptions<GameCoordinatorOptions>()
            .Bind(configuration.GetSection(GameCoordinatorOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHostedService<GameCoordinator>();
        services.AddSingleton<IGameService, GameService>();
    }
}
