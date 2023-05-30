using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Hubs;
using OhMyWord.Domain.Contracts.Events;

namespace OhMyWord.Api.Handlers;

public class LetterHintAddedHandler : IEventHandler<LetterHintAddedEvent>
{
    private readonly IHubContext<GameHub, IGameHub> gameHubContext;

    public LetterHintAddedHandler(IHubContext<GameHub, IGameHub> gameHubContext)
    {
        this.gameHubContext = gameHubContext;
    }

    public Task HandleAsync(LetterHintAddedEvent eventModel, CancellationToken cancellationToken = new())
        => gameHubContext.Clients.All.SendLetterHint(eventModel.LetterHint, cancellationToken);
}
