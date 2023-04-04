using Microsoft.Extensions.DependencyInjection;
using OhMyWord.Domain.Services;

namespace OhMyWord.Domain.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddSingleton<IDefinitionsService, DefinitionsService>();
        services.AddSingleton<IUsersService, UsersService>();
        services.AddSingleton<IPlayerService, PlayerService>();
        services.AddSingleton<IWordsService, WordsService>();

        return services;
    }
}
