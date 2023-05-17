using OhMyWord.Api.Events.LetterHintAdded;
using OhMyWord.Api.Events.RoundEnded;
using OhMyWord.Api.Events.RoundStarted;
using OhMyWord.Domain.Models;
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
    private readonly IServiceProvider serviceProvider;

    private IRoundService? roundService;
    private int roundNumber;
    private IReadOnlyList<string> allWordIds = new List<string>();
    private Stack<string> shuffledWordIds = new();

    public GameState GameState => new()
    {
        RoundActive = roundService?.IsRoundActive ?? default,
        RoundNumber = roundService?.RoundNumber ?? default,
        RoundId = roundService?.RoundId ?? default,
        IntervalStart = roundService?.IntervalStart ?? default,
        IntervalEnd = roundService?.IntervalEnd ?? default,
        WordHint = roundService?.WordHint ?? default,
    };

    public GameService(ILogger<GameService> logger, IWordsService wordsService, IPlayerService playerService,
        IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.wordsService = wordsService;
        this.playerService = playerService;
        this.serviceProvider = serviceProvider;
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

            // assign round service
            using var scope = serviceProvider.CreateScope();
            roundService = scope.ServiceProvider.GetRequiredService<IRoundService>();

            // start new round
            var word = await GetNextWordAsync(cancellationToken);
            var (startData, roundCancellationToken) = roundService.StartRound(word, ++roundNumber);
            await new RoundStartedEvent(startData).PublishAsync(cancellation: cancellationToken);

            // send all letter hints
            try
            {
                await SendLetterHintsAsync(startData, word, roundCancellationToken);
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation("Round {RoundNumber} has been terminated early. Reason: {EndReason}",
                    roundService.RoundNumber, roundService.LastEndReason);
            }

            // end current round
            var endData = roundService.EndRound();
            await Task.WhenAll(
                new RoundEndedEvent(endData).PublishAsync(cancellation: cancellationToken),
                Task.Delay(endData.PostRoundDelay, cancellationToken));            
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

    private static async Task SendLetterHintsAsync(RoundStartData data, Word word, CancellationToken cancellationToken)
    {
        var wordHint = data.WordHint;
        var duration = data.EndDate - data.StartDate;
        var letterDelay = duration / word.Length;
        var previousIndices = new List<int>();

        while (previousIndices.Count < word.Length && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(letterDelay, cancellationToken);

            int index;
            do index = Random.Shared.Next(word.Length);
            while (previousIndices.Contains(index));
            previousIndices.Add(index);

            var letterHint = word.GetLetterHint(index + 1);
            wordHint.AddLetterHint(letterHint);
            await new LetterHintAddedEvent(letterHint).PublishAsync(cancellation: cancellationToken);
        }
    }

    public async Task<int> ProcessGuessAsync(string connectionId, Guid roundId, string value)
    {
        if (roundService is null) return 0;

        // validate round state
        if (!roundService.IsRoundActive || roundId != roundService.RoundId) return 0;

        // compare value to current word value
        var isCorrect = string.Equals(value, roundService.Word.Id, StringComparison.InvariantCultureIgnoreCase);
        if (!isCorrect) return 0;

        var player = playerService.GetPlayer(connectionId);

        roundService.IncrementGuessCount(player.Id);

        const int pointsToAward = 100; // TODO: Calculate points dynamically
        var pointsAwarded = roundService.AwardPoints(player.Id, pointsToAward);
        if (!pointsAwarded)
            logger.LogWarning("Zero points were awarded to player with ID: {PlayerId}", player.Id);

        await playerService.IncrementPlayerScoreAsync(player.Id,
            pointsToAward); // TODO: Write to database after round end

        // end round if all players have been awarded points
        if (roundService.AllPlayersGuessed)
            roundService.EndRound(RoundEndReason.AllPlayersGuessed);

        return pointsToAward;
    }

    public void AddPlayer(string playerId) => roundService?.AddPlayer(playerId);

    public void RemovePlayer(string playerId)
    {
        if (roundService is null) return;

        // player left while round wasn't active
        if (!roundService.IsRoundActive)
            return;

        // last visitor left while round active
        if (playerService.PlayerCount == 0)
            roundService.EndRound(RoundEndReason.NoPlayersLeft);
    }
}
