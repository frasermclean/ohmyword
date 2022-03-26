using MediatR;
using Microsoft.AspNetCore.SignalR;
using WhatTheWord.Api.Mediator.Requests.Game;
using WhatTheWord.Api.Responses.Game;

namespace WhatTheWord.Api.Hubs;

public interface IGameHub
{
    Task SendHint(HintResponse response);
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

    public async Task<HintResponse> GetHint(GetHintRequest request)
    {
        var response = await mediator.Send(request);
        return response;
    }

    public async Task<GuessWordResponse> GuessWord(GuessWordRequest request)
    {
        var response = await mediator.Send(request);
        return response;
    }

    public Task SendHint(HintResponse response) => Clients.All.SendHint(response);
}
