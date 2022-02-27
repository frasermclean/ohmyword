using MediatR;
using Microsoft.AspNetCore.SignalR;
using WhatTheWord.Domain.Requests.Game;
using WhatTheWord.Domain.Responses.Game;

namespace WhatTheWord.Api.Hubs;

public interface IGameHub
{
}

public class GameHub : Hub<IGameHub>
{
    private readonly ILogger<GameHub> logger;
    private readonly IMediator mediator;

    public GameHub(ILogger<GameHub> logger, IMediator mediator)
    {
        this.logger = logger;
        this.mediator = mediator;
    }

    public async Task<GetHintResponse> GetHint()
    {
        return await mediator.Send(new GetHintRequest());
    }

    public override Task OnConnectedAsync()
    {
        logger.LogInformation("Client connected.");
        return Task.CompletedTask;
    }
}
