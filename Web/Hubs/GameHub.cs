using MediatR;
using Microsoft.AspNetCore.SignalR;
using WhatTheWord.Domain.Requests.Game;
using WhatTheWord.Domain.Responses.Game;

namespace WhatTheWord.Api.Hubs;

public interface IGameHub
{
    Task<RegisterClientResponse> Register(RegisterClientRequest request);
    Task<GetHintResponse> GetHint(GetHintRequest request);
}

public class GameHub : Hub<IGameHub>, IGameHub
{
    private readonly ILogger<GameHub> logger;
    private readonly IMediator mediator;

    public GameHub(ILogger<GameHub> logger, IMediator mediator)
    {
        this.logger = logger;
        this.mediator = mediator;
    }

    public async Task<RegisterClientResponse> Register(RegisterClientRequest request)
    {
        logger.LogInformation("Attempting to register client with visitor ID: {visitorId}", request.VisitorId);
        var response = await mediator.Send(request);
        return response;
    }

    public async Task<GetHintResponse> GetHint(GetHintRequest request)
    {
        var response = await mediator.Send(request);
        return response;
    }

    public async Task<GuessWordResponse> GuessWord(GuessWordRequest request)
    {
        var response = await mediator.Send(request);
        return response;
    }

    public override Task OnConnectedAsync()
    {
        logger.LogInformation("Client connected.");
        return Task.CompletedTask;
    }
}
