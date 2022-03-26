using MediatR;
using OhMyWord.Api.Mediator.Handlers.Game;

namespace OhMyWord.Api.Registration;

public static class MediatorServiceRegistration
{
    public static IServiceCollection AddMediatorService(this IServiceCollection services)
    {
        services.AddMediatR(typeof(RegisterPlayerHandler));
        return services;
    }
}
