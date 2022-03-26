using MediatR;
using WhatTheWord.Domain.Handlers.Game;

namespace WhatTheWord.Api.Configuration;

public static class MediatorService
{
    public static IServiceCollection AddMediatorService(this IServiceCollection services)
    {
        services.AddMediatR(typeof(RegisterClientHandler));
        return services;
    }
}
