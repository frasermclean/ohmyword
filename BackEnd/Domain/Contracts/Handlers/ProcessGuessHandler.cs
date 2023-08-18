using FastEndpoints;
using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Contracts.Commands;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services.State;

namespace OhMyWord.Domain.Contracts.Handlers;

public class ProcessGuessHandler : ICommandHandler<ProcessGuessCommand, ProcessGuessResult>
{
    private readonly ILogger<ProcessGuessHandler> logger;
    private readonly IRoundState roundState;
    private readonly IPlayerState playerState;

    public ProcessGuessHandler(ILogger<ProcessGuessHandler> logger, IRoundState roundState, IPlayerState playerState)
    {
        this.logger = logger;
        this.roundState = roundState;
        this.playerState = playerState;
    }

    public Task<ProcessGuessResult> ExecuteAsync(ProcessGuessCommand command,
        CancellationToken cancellationToken = new())
    {
        var player = playerState.GetPlayerByConnectionId(command.ConnectionId);
        if (player is null)
        {
            logger.LogError("Player not found. ConnectionId: {ConnectionId}", command.ConnectionId);
            return Task.FromResult(ProcessGuessResult.Default);
        }

        var result = roundState.ProcessGuess(player.Id, command.RoundId, command.Value);
        return Task.FromResult(new ProcessGuessResult
        {
            IsCorrect = result.IsSuccess,
            PointsAwarded = result.ValueOrDefault,
            Message = result.IsFailed ? result.Errors.First().Message : string.Empty
        });
    }
}
