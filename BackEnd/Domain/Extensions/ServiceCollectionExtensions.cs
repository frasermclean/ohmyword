using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OhMyWord.Domain.Options;
using OhMyWord.Domain.Services;

namespace OhMyWord.Domain.Extensions;

public static class ServiceCollectionExtensions
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
        services.AddSingleton<IRoundFactory, RoundFactory>();        
        services.AddSingleton<IWordsService, WordsService>();

        return services;
    }
}
