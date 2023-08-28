using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OhMyWord.Logic.Options;
using OhMyWord.Logic.Services;
using OhMyWord.Logic.Services.State;

namespace OhMyWord.Logic.DependencyInjection;

public static class DomainServicesRegistration
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RoundOptions>()
            .Bind(configuration.GetSection(RoundOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IDefinitionsService, DefinitionsService>();
        services.AddSingleton<IGeoLocationService, GeoLocationService>();
        services.AddSingleton<IPlayerService, PlayerService>();
        services.AddSingleton<IRoundService, RoundService>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<IWordsService, WordsService>();
        services.AddSingleton<IWordQueueService, WordQueueService>();

        // state management
        services.AddSingleton<IRootState, RootState>();
        services.AddSingleton<IPlayerState, PlayerState>();
        services.AddSingleton<IRoundState, RoundState>();
        services.AddSingleton<ISessionState, SessionState>();

        return services;
    }
}
