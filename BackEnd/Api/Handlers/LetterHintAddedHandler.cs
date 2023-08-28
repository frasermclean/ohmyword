using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Hubs;
using OhMyWord.Api.Models;
using OhMyWord.Logic.Contracts.Events;

namespace OhMyWord.Api.Handlers;

public class LetterHintAddedHandler : IEventHandler<LetterHintAddedEvent>
{
    private readonly IHubContext<GameHub, IGameHub> gameHubContext;

    public LetterHintAddedHandler(IHubContext<GameHub, IGameHub> gameHubContext)
    {
        this.gameHubContext = gameHubContext;
    }

    public async Task HandleAsync(LetterHintAddedEvent eventModel, CancellationToken cancellationToken)
    {
        var response = LetterHintResponse.FromLetterHint(eventModel.LetterHint);
        await gameHubContext.Clients.All.SendLetterHint(response, cancellationToken);
    }
}
