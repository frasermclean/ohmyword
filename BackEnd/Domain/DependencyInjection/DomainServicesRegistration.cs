using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OhMyWord.Domain.Options;
using OhMyWord.Domain.Services;

namespace OhMyWord.Domain.DependencyInjection;

public static class DomainServicesRegistration
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RoundOptions>()
            .Bind(configuration.GetSection(RoundOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IDefinitionsService, DefinitionsService>();
        services.AddSingleton<IUsersService, UsersService>();
        services.AddSingleton<IGeoLocationService, GeoLocationService>();
        services.AddSingleton<IPlayerService, PlayerService>();
        services.AddSingleton<IPlayerState, PlayerState>();
        services.AddSingleton<IRoundService, RoundService>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<IStateManager, StateManager>();
        services.AddSingleton<IWordsService, WordsService>();
        services.AddSingleton<IWordQueueService, WordQueueService>();

        return services;
    }
}
