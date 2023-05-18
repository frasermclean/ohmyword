using Microsoft.Extensions.Options;
using OhMyWord.Api.Events.LetterHintAdded;
using OhMyWord.Api.Events.RoundEnded;
using OhMyWord.Api.Events.RoundStarted;
using OhMyWord.Api.Models;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Options;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Models.Entities;

namespace OhMyWord.Api.Services;

public interface IGameService
{
    /// <summary>
    /// Snapshot of the current game state
    /// </summary>    
    GameState GameState { get; }

    Task ExecuteGameLoopAsync(CancellationToken cancellationToken);
    Task<int> ProcessGuessAsync(string connectionId, Guid roundId, string value);
    void AddPlayer(string playerId);
    void RemovePlayer(string playerId);
}

public class GameService : IGameService
{
    private readonly ILogger<GameService> logger;
    private readonly IWordsService wordsService;
    private readonly IPlayerService playerService;
    private readonly IRoundFactory roundFactory;

    private readonly TimeSpan postRoundDelay;

    private bool isRoundActive;
    private int roundNumber;
    private Round round = Round.Default;
    private IReadOnlyList<string> allWordIds = new List<string>();
    private Stack<string> shuffledWordIds = new();

    public GameState GameState => new()
    {
        RoundActive = isRoundActive,
        RoundNumber = round.Number,
        RoundId = round.Id,
        IntervalStart = isRoundActive ? round.StartDate : default,
        IntervalEnd = isRoundActive ? round.EndDate : default,
        WordHint = isRoundActive ? round.WordHint : default,
    };

    public GameService(ILogger<GameService> logger, IOptions<RoundOptions> roundOptions, IWordsService wordsService,
        IPlayerService playerService, IRoundFactory roundFactory)
    {
        this.logger = logger;
        this.wordsService = wordsService;
        this.playerService = playerService;
        this.roundFactory = roundFactory;

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

            await ExecuteRoundAsync(cancellationToken);

            // end current round            
            await Task.WhenAll(
                new RoundEndedEvent
                {
                    RoundId = round.Id,
                    Word = round.Word.Id,
                    EndReason = round.EndReason,
                    NextRoundStart = DateTime.UtcNow + postRoundDelay,
                    Scores = round.GetPlayerData().Select(data =>
                    {
                        var player = playerService.GetPlayerById(data.PlayerId);
                        return new ScoreLine
                        {
                            PlayerName = string.Empty, // TODO: Calculate player name
                            ConnectionId = player?.ConnectionId ?? string.Empty,
                            CountryCode = string.Empty, // TODO: Calculate country code
                            PointsAwarded = data.PointsAwarded,
                            GuessCount = data.GuessCount,
                        };
                    })
                }.PublishAsync(cancellation: cancellationToken),
                Task.Delay(postRoundDelay, cancellationToken));
        }
    }

    private async Task ExecuteRoundAsync(CancellationToken cancellationToken)
    {
        // start new round
        var word = await GetNextWordAsync(cancellationToken);
        using (round = roundFactory.CreateRound(word, ++roundNumber))
        {
            isRoundActive = true;
            await new RoundStartedEvent
            {
                RoundNumber = round.Number,
                RoundId = round.Id,
                WordHint = round.WordHint,
                StartDate = round.StartDate,
                EndDate = default
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

            isRoundActive = false;
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

    public async Task<int> ProcessGuessAsync(string connectionId, Guid roundId, string value)
    {
        // validate round state
        if (!isRoundActive || roundId != round.Id) return 0;

        // compare value to current word value
        var isCorrect = string.Equals(value, round.Word.Id, StringComparison.InvariantCultureIgnoreCase);
        if (!isCorrect) return 0;

        var player = playerService.GetPlayerByConnectionId(connectionId);
        if (player is null)
        {
            logger.LogError("Player not found. ConnectionId: {ConnectionId}", connectionId);
            return 0;
        }

        round.IncrementGuessCount(player.Id);

        const int pointsToAward = 100; // TODO: Calculate points dynamically
        var pointsAwarded = round.AwardPoints(player.Id, pointsToAward);
        if (!pointsAwarded)
            logger.LogWarning("Zero points were awarded to player with ID: {PlayerId}", player.Id);

        await playerService.IncrementPlayerScoreAsync(player.Id,
            pointsToAward); // TODO: Write to database after round end

        // end round if all players have been awarded points
        if (round.AllPlayersGuessed)
            round.EndRound(RoundEndReason.AllPlayersGuessed);

        return pointsToAward;
    }

    public void AddPlayer(string playerId)
    {
        if (isRoundActive)
            round.AddPlayer(playerId);
    }

    public void RemovePlayer(string playerId)
    {
        // player left while round wasn't active
        if (!isRoundActive)
            return;

        // last visitor left while round active
        if (playerService.PlayerCount == 0)
            round.EndRound(RoundEndReason.NoPlayersLeft);
    }
}
