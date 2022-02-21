using MediatR;
using WhatTheWord.Domain.Processing.Clients;

namespace WhatTheWord.Api.Services;

public static class MediatorService
{
    public static IServiceCollection AddMediatorService(this IServiceCollection services)
    {
        services.AddMediatR(typeof(RegisterClientHandler));
        return services;
    }
}
