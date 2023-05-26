using MediatR;
using OhMyWord.Domain.Contracts.Requests;
using OhMyWord.Domain.Contracts.Results;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;

namespace OhMyWord.Domain.Contracts.Handlers;

public class RegisterPlayerHandler : IRequestHandler<RegisterPlayerRequest, RegisterPlayerResult>
{
    private readonly IPlayerService playerService;
    private readonly IStateManager stateManager;

    public RegisterPlayerHandler(IPlayerService playerService, IStateManager stateManager)
    {
        this.playerService = playerService;
        this.stateManager = stateManager;
    }
    
    public async Task<RegisterPlayerResult> Handle(RegisterPlayerRequest request, CancellationToken cancellationToken)
    {
        var player = await playerService.GetOrCreatePlayerAsync(request);
        var isSuccessful = stateManager.PlayerState.AddPlayer(player);

        return new RegisterPlayerResult
        {
            IsSuccessful = isSuccessful,
            PlayerId = player.Id,
            PlayerCount = stateManager.PlayerState.PlayerCount,
            Score = player.Score,
            RegistrationCount = player.RegistrationCount,
            StateSnapshot = GetGameState()
        };
    }
    
    private StateSnapshot GetGameState() => new()
    {
        RoundActive = stateManager.SessionState == SessionState.RoundActive,
        RoundNumber = stateManager.Round?.Number ?? default,
        RoundId = stateManager.Round?.Id ?? default,
        IntervalStart = stateManager.IntervalStart,
        IntervalEnd = stateManager.IntervalEnd,
        WordHint = stateManager.Round?.WordHint,
    };
}
