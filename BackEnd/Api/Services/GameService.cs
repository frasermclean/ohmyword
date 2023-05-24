using Microsoft.Extensions.Options;
using OhMyWord.Api.Events.LetterHintAdded;
using OhMyWord.Api.Events.RoundEnded;
using OhMyWord.Api.Events.RoundStarted;
using OhMyWord.Api.Models;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Options;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Models.Entities;
using System.Net;

namespace OhMyWord.Api.Services;

public interface IGameService
{
    Task ExecuteGameLoopAsync(CancellationToken cancellationToken);


    void RemovePlayer(string playerId);

    Task<RegisterPlayerResult> RegisterPlayerAsync(string connectionId, string visitorId, IPAddress ipAddress,
        Guid? userId, CancellationToken cancellationToken = default);

    Task<ProcessGuessResult> ProcessGuessAsync(string connectionId, Guid roundId, string value);
}

public class GameService : IGameService
{
    private readonly ILogger<GameService> logger;
    private readonly IWordsService wordsService;
    private readonly IPlayerService playerService;
    private readonly IRoundService roundService;

    private readonly TimeSpan postRoundDelay;


    private int roundNumber;
    private IReadOnlyList<string> allWordIds = new List<string>();
    private Stack<string> shuffledWordIds = new();

    private Guid SessionId { get; } = Guid.NewGuid(); // TODO: Refactor to use a session service
    private Round Round { get; set; } = Round.Default;
    private bool IsRoundActive => Round != Round.Default;

    public GameService(ILogger<GameService> logger, IOptions<RoundOptions> roundOptions, IWordsService wordsService,
        IPlayerService playerService, IRoundService roundService)
    {
        this.logger = logger;
        this.wordsService = wordsService;
        this.playerService = playerService;
        this.roundService = roundService;

        postRoundDelay = TimeSpan.FromSeconds(roundOptions.Value.PostRoundDelay);
    }

    public async Task ExecuteGameLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // sleep while there are no visitors
            if (playerService.PlayerCount == 0)
            {
                await Task.Delay(1000, cancellationToken);
                continue;
            }

            using (Round = await roundService.CreateRoundAsync(++roundNumber, SessionId, cancellationToken))
            {
                await ExecuteRoundAsync(Round, cancellationToken);

                // end current round            
                await Task.WhenAll(CreateRoundEndedEvent(Round).PublishAsync(cancellation: cancellationToken),
                    Task.Delay(postRoundDelay, cancellationToken));
            }

            Round = Round.Default;
        }
    }

    private async Task ExecuteRoundAsync(Round round, CancellationToken cancellationToken)
    {
        await new RoundStartedEvent
        {
            RoundNumber = round.Number,
            RoundId = round.Id,
            WordHint = round.WordHint,
            StartDate = round.StartDate,
            EndDate = round.EndDate
        }.PublishAsync(cancellation: cancellationToken);

        // send all letter hints
        try
        {
            await SendLetterHintsAsync(round, round.CancellationToken);
        }
        catch (TaskCanceledException)
        {
            logger.LogInformation("Round {RoundNumber} has been terminated early. Reason: {EndReason}",
                round.Number, round.EndReason);
        }
        finally
        {
            await roundService.SaveRoundAsync(round, cancellationToken);
        }
    }

    private async Task<Word> GetNextWordAsync(CancellationToken cancellationToken)
    {
        if (allWordIds.Count == 0)
            allWordIds = await wordsService
                .GetAllWordIds(cancellationToken)
                .ToListAsync(cancellationToken);

        if (allWordIds.Count == 0)
        {
            logger.LogError("No words found in database");
            return Word.Default;
        }

        if (shuffledWordIds.Count == 0)
        {
            logger.LogInformation("Detected empty shuffled word stack. Shuffling words: {Count}", allWordIds.Count);
            shuffledWordIds = new Stack<string>(allWordIds.OrderBy(_ => Random.Shared.Next()));
        }

        var wordId = shuffledWordIds.Pop();
        var result = await wordsService.GetWordAsync(wordId, cancellationToken);

        return result.Match(
            word => word,
            _ =>
            {
                logger.LogError("Word not found in database. WordId: {WordId}", wordId);
                return Word.Default;
            });
    }

    private static async Task SendLetterHintsAsync(Round round, CancellationToken cancellationToken)
    {
        var previousIndices = new List<int>();

        while (previousIndices.Count < round.Word.Length && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay((round.EndDate - round.StartDate) / round.Word.Length, round.CancellationToken);

            int index;
            do index = Random.Shared.Next(round.Word.Length);
            while (previousIndices.Contains(index));
            previousIndices.Add(index);

            var letterHint = round.Word.GetLetterHint(index + 1);
            round.WordHint.AddLetterHint(letterHint);
            await new LetterHintAddedEvent(letterHint).PublishAsync(cancellation: cancellationToken);
        }
    }

    private RoundEndedEvent CreateRoundEndedEvent(Round round) => new()
    {
        Word = round.Word.Id,
        EndReason = round.EndReason,
        RoundId = round.Id,
        DefinitionId = round.WordHint.DefinitionId,
        NextRoundStart = DateTime.UtcNow + postRoundDelay,
        Scores = round.GetPlayerData()
            .Where(data => data.PointsAwarded > 0)
            .Select(data =>
            {
                var player = playerService.GetPlayerById(data.PlayerId);
                return new ScoreLine
                {
                    PlayerName = string.Empty, // TODO: Calculate player name
                    ConnectionId = player?.ConnectionId ?? string.Empty,
                    CountryCode = string.Empty, // TODO: Calculate country code
                    PointsAwarded = data.PointsAwarded,
                    GuessCount = data.GuessCount,
                    GuessTimeMilliseconds = data.GuessTime.TotalMilliseconds
                };
            })
    };

    public async Task<RegisterPlayerResult> RegisterPlayerAsync(string connectionId, string visitorId,
        IPAddress ipAddress,
        Guid? userId, CancellationToken cancellationToken)
    {
        var player = await playerService.AddPlayerAsync(visitorId, connectionId, ipAddress, userId);

        AddPlayer(player.Id); // TODO: Move to SessionManager

        return new RegisterPlayerResult
        {
            PlayerCount = playerService.PlayerCount, Score = player.Score, GameState = GetGameState()
        };
    }

    public async Task<ProcessGuessResult> ProcessGuessAsync(string connectionId, Guid roundId, string value)
    {
        // validate round state
        if (!IsRoundActive || roundId != Round.Id) return ProcessGuessResult.Default;

        // compare value to current word value
        var isCorrect = string.Equals(value, Round.Word.Id, StringComparison.InvariantCultureIgnoreCase);
        if (!isCorrect) return ProcessGuessResult.Default;

        var player = playerService.GetPlayerByConnectionId(connectionId);
        if (player is null)
        {
            logger.LogError("Player not found. ConnectionId: {ConnectionId}", connectionId);
            return ProcessGuessResult.Default;
        }

        Round.IncrementGuessCount(player.Id);

        const int pointsToAward = 100; // TODO: Calculate points dynamically
        var pointsAwarded = Round.AwardPoints(player.Id, pointsToAward);
        if (!pointsAwarded)
            logger.LogWarning("Zero points were awarded to player with ID: {PlayerId}", player.Id);

        await playerService.IncrementPlayerScoreAsync(player.Id,
            pointsToAward); // TODO: Write to database after round end

        // end round if all players have been awarded points
        if (Round.AllPlayersGuessed)
            Round.EndRound(RoundEndReason.AllPlayersGuessed);

        return new ProcessGuessResult { IsCorrect = isCorrect, PointsAwarded = pointsToAward };
    }

    public void AddPlayer(string playerId)
    {
        if (IsRoundActive)
            Round.AddPlayer(playerId);
    }

    public void RemovePlayer(string playerId)
    {
        // player left while round wasn't active
        if (!IsRoundActive)
            return;

        // last visitor left while round active
        if (playerService.PlayerCount == 0)
            Round.EndRound(RoundEndReason.NoPlayersLeft);
    }

    private GameState GetGameState() => new()
    {
        RoundActive = Round != Round.Default,
        RoundNumber = Round.Number,
        RoundId = Round.Id,
        IntervalStart = IsRoundActive ? Round.StartDate : default,
        IntervalEnd = IsRoundActive ? Round.EndDate : default,
        WordHint = IsRoundActive ? Round.WordHint : default,
    };
}
