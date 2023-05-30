using FastEndpoints;
using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Contracts.Commands;
using OhMyWord.Domain.Contracts.Results;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Models.Entities;

namespace OhMyWord.Domain.Contracts.Handlers;

public class ProcessGuessHandler : ICommandHandler<ProcessGuessCommand, ProcessGuessResult>
{
    private readonly ILogger<ProcessGuessHandler> logger;
    private readonly IStateManager stateManager;
    private readonly IPlayerService playerService;

    public ProcessGuessHandler(ILogger<ProcessGuessHandler> logger, IStateManager stateManager,
        IPlayerService playerService)
    {
        this.logger = logger;
        this.stateManager = stateManager;
        this.playerService = playerService;
    }

    public async Task<ProcessGuessResult> ExecuteAsync(ProcessGuessCommand command,
        CancellationToken cancellationToken = new())
    {
        // validate round state
        var round = stateManager.Round;
        if (round is null) return ProcessGuessResult.Default;
        if (stateManager.SessionState != SessionState.RoundActive || command.RoundId != round.Id)
            return ProcessGuessResult.Default;

        // compare value to current word value
        var isMatch = IsMatch(round.Word.Id, command.Value);
        if (!isMatch) return ProcessGuessResult.Default;

        var player = stateManager.PlayerState.GetPlayerByConnectionId(command.ConnectionId);
        if (player is null)
        {
            logger.LogError("Player not found. ConnectionId: {ConnectionId}", command.ConnectionId);
            return ProcessGuessResult.Default;
        }

        round.IncrementGuessCount(player.Id); // TODO: Move this up and validate guess count

        const int pointsToAward = 100; // TODO: Calculate points dynamically
        var pointsAwarded = round.AwardPoints(player.Id, pointsToAward);
        if (!pointsAwarded)
            logger.LogWarning("Zero points were awarded to player with ID: {PlayerId}", player.Id);

        await playerService.IncrementPlayerScoreAsync(player.Id,
            pointsToAward); // TODO: Write to database after round end

        // end round if all players have been awarded points
        if (round.AllPlayersGuessed)
            round.EndRound(RoundEndReason.AllPlayersGuessed);

        return new ProcessGuessResult { IsCorrect = isMatch, PointsAwarded = pointsToAward };
    }

    private static bool IsMatch(string value1, string value2) =>
        string.Equals(value1, value2, StringComparison.InvariantCultureIgnoreCase);
}
