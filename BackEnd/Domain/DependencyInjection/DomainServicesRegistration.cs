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
        services.AddSingleton<IRoundManager, RoundManager>();
        services.AddSingleton<ISessionManager, SessionManager>();
        services.AddSingleton<IStateProvider, StateProvider>();
        services.AddSingleton<IWordsService, WordsService>();

        return services;
    }
}
