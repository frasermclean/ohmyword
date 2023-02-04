using FastEndpoints;
using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Hubs;

namespace OhMyWord.Api.Events.LetterHintAdded;

public class LetterHintAddedHandler : IEventHandler<LetterHintAddedEvent>
{
    private readonly IHubContext<GameHub, IGameHub> gameHubContext;

    public LetterHintAddedHandler(IHubContext<GameHub, IGameHub> gameHubContext)
    {
        this.gameHubContext = gameHubContext;
    }

    public Task HandleAsync(LetterHintAddedEvent eventModel, CancellationToken cancellationToken)
        => gameHubContext.Clients.All.SendLetterHint(eventModel.LetterHint);
}
