using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Hubs;
using OhMyWord.Api.Models;
using OhMyWord.Domain.Contracts.Events;

namespace OhMyWord.Api.Handlers;

public class RoundEndedHandler : IEventHandler<RoundEndedEvent>
{
    private readonly IHubContext<GameHub, IGameHub> gameHubContext;

    public RoundEndedHandler(IHubContext<GameHub, IGameHub> gameHubContext)
    {
        this.gameHubContext = gameHubContext;
    }

    public async Task HandleAsync(RoundEndedEvent eventModel, CancellationToken cancellationToken)
    {
        var response = RoundEndedResponse.FromRoundSummary(eventModel.Summary);
        await gameHubContext.Clients.All.SendRoundEnded(response, cancellationToken);
    }
}
