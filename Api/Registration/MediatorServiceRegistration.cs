using MediatR;
using WhatTheWord.Api.Mediator.Handlers.Game;

namespace WhatTheWord.Api.Registration;

public static class MediatorServiceRegistration
{
    public static IServiceCollection AddMediatorService(this IServiceCollection services)
    {
        services.AddMediatR(typeof(RegisterClientHandler));
        return services;
    }
}
