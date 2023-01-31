using OhMyWord.Api.Options;
using OhMyWord.Api.Services;

namespace OhMyWord.Api.Registration;

public static class GameServicesRegistration
{
    public static void AddGameServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<GameServiceOptions>()
            .Bind(configuration.GetSection(GameServiceOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHostedService<GameCoordinator>();
        services.AddSingleton<IWordsService, WordsService>();
        services.AddSingleton<IGameService, GameService>();
        services.AddSingleton<IVisitorService, VisitorService>();
    }
}
