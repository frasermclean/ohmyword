using MediatR;
using Microsoft.AspNetCore.SignalR;
using OhMyWord.Api.Mediator.Requests.Game;
using OhMyWord.Api.Responses.Game;
using OhMyWord.Api.Services;

namespace OhMyWord.Api.Hubs;

public interface IGameHub
{
    Task SendHint(HintResponse response);
}

public class GameHub : Hub<IGameHub>, IGameHub
{
    private readonly ILogger<GameHub> logger;
    private readonly IMediator mediator;
    private readonly IGameService gameService;

    public GameHub(ILogger<GameHub> logger, IMediator mediator, IGameService gameService)
    {
        this.logger = logger;
        this.mediator = mediator;
        this.gameService = gameService;
    }

    public override Task OnConnectedAsync()
    {
        logger.LogDebug("New client connected from connection ID: {connectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogDebug("Client disconnected. Connection ID: {connectionId}", Context.ConnectionId);
        await gameService.UnregisterPlayerAsync(Context.ConnectionId);
    }

    public async Task<RegisterPlayerResponse> Register(string visitorId)
    {
        logger.LogInformation("Attempting to register client with visitor ID: {visitorId}", visitorId);

        return await gameService.RegisterPlayerAsync(new RegisterPlayerRequest
        {
            VisitorId = visitorId,
            ConnectionId = Context.ConnectionId
        });
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
