using Microsoft.Extensions.Options;
using OhMyWord.Api.Events.GameStateChanged;
using OhMyWord.Api.Events.LetterHintAdded;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Options;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Enums;

namespace OhMyWord.Api.Services;

public interface IGameService
{
    /// <summary>
    /// Snapshot of the current game state
    /// </summary>    
    GameState State { get; }

    Round Round { get; }
    GameServiceOptions Options { get; }

    Task ExecuteGameAsync(CancellationToken gameCancellationToken);
    Task<int> ProcessGuessAsync(string connectionId, Guid roundId, string value);
    void AddVisitor(string visitorId);
    void RemoveVisitor(string visitorId);
}

public class GameService : IGameService
{
    private readonly ILogger<GameService> logger;
    private readonly IWordsService wordsService;
    private readonly IVisitorService visitorService;

    private IReadOnlyList<string> allWordIds = new List<string>();
    private Stack<string> shuffledWordIds = new();

    public GameState State { get; private set; } = new();
    public Round Round { get; private set; } = Round.Default;
    public GameServiceOptions Options { get; }

    public GameService(ILogger<GameService> logger, IWordsService wordsService, IVisitorService visitorService,
        IOptions<GameServiceOptions> options)
    {
        this.logger = logger;
        this.wordsService = wordsService;
        this.visitorService = visitorService;

        Options = options.Value;
    }

    public async Task ExecuteGameAsync(CancellationToken gameCancellationToken)
    {
        while (!gameCancellationToken.IsCancellationRequested)
        {
            // sleep while there are no visitors
            if (visitorService.VisitorCount == 0)
            {
                await Task.Delay(1000, gameCancellationToken);
                continue;
            }

            // start new round
            Round = await CreateRoundAsync(gameCancellationToken);
            await UpdateStateAsync(true, Round.Number, Round.Id, Round.EndTime, Round.WordHint);

            logger.LogDebug(
                "Round: {RoundNumber} has started with {VisitorCount} visitors. Current word is: {WordId}. Round duration: {Seconds} seconds",
                Round.Number, Round.VisitorCount, Round.Word.Id, Round.Duration.Seconds);

            // send all letter hints
            try
            {
                await SendLetterHintsAsync(Round);
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation("Round has been terminated early. Reason: {EndReason}", Round.EndReason);
            }

            // end current round
            var postRoundDelay = TimeSpan.FromSeconds(Options.PostRoundDelay);
            var nextRoundStart = DateTime.UtcNow + postRoundDelay;
            await UpdateStateAsync(false, Round.Number, Round.Id, nextRoundStart);


            Round.Dispose();

            logger.LogDebug("Round: {Number} has ended. Post round delay is: {Seconds} seconds", Round.Number,
                postRoundDelay.Seconds);

            await Task.Delay(postRoundDelay, gameCancellationToken);
        }
    }

    private async Task<Round> CreateRoundAsync(CancellationToken cancellationToken)
    {
        var roundNumber = Round.Number + 1;
        var word = await GetNextWordAsync(cancellationToken);
        var duration = TimeSpan.FromSeconds(word.Length * Options.LetterHintDelay);

        return new Round(roundNumber, word, duration, visitorService.VisitorIds);
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
        var word = await wordsService.GetWordAsync(wordId, cancellationToken);

        if (word is not null) return word;

        logger.LogError("Word not found in database. WordId: {WordId}", wordId);
        return Word.Default;
    }

    private static async Task SendLetterHintsAsync(Round round)
    {
        var word = round.Word;
        var wordHint = round.WordHint;
        var letterDelay = round.Duration / word.Length;
        var previousIndices = new List<int>();

        while (previousIndices.Count < word.Length && !round.CancellationToken.IsCancellationRequested)
        {
            await Task.Delay(letterDelay, round.CancellationToken);

            int index;
            do index = Random.Shared.Next(word.Length);
            while (previousIndices.Contains(index));
            previousIndices.Add(index);

            var letterHint = word.GetLetterHint(index + 1);
            wordHint.AddLetterHint(letterHint);
            await new LetterHintAddedEvent(letterHint).PublishAsync();
        }
    }

    public async Task<int> ProcessGuessAsync(string connectionId, Guid roundId, string value)
    {
        var visitor = visitorService.GetVisitor(connectionId);

        // if round is not active then immediately return false
        if (!State.RoundActive || roundId != Round.Id) return 0;

        // compare value to current word value
        if (!string.Equals(value, Round.Word.Id, StringComparison.InvariantCultureIgnoreCase))
            return 0;

        var guessCountIncremented = Round.IncrementGuessCount(visitor.Id);
        if (!guessCountIncremented)
            logger.LogWarning("Couldn't increment guess count of visitor with ID: {VisitorId}", visitor.Id);

        var pointsAwarded = Round.AwardVisitor(visitor.Id);
        if (pointsAwarded == 0)
            logger.LogWarning("Zero points were awarded to visitor with ID: {VisitorId}", visitor.Id);

        await visitorService.IncrementVisitorScoreAsync(visitor.Id, pointsAwarded);

        // end round if all visitors have been awarded points
        if (Round.AllVisitorsAwarded)
            Round.EndRound(RoundEndReason.AllVisitorsAwarded);

        return pointsAwarded;
    }

    public void AddVisitor(string visitorId)
    {
        // visitor joined while round wasn't active
        if (!State.RoundActive)
            return;

        var wasAdded = Round.AddVisitor(visitorId);
        if (!wasAdded)
            logger.LogError("Couldn't add visitor with ID {VisitorId} to round", visitorId);
    }

    public void RemoveVisitor(string visitorId)
    {
        // visitor left while round wasn't active
        if (!State.RoundActive)
            return;

        var wasRemoved = Round.RemoveVisitor(visitorId);
        if (!wasRemoved)
            logger.LogError("Couldn't remove visitor with ID {VisitorId} from round", visitorId);

        // last visitor left while round active
        if (visitorService.VisitorCount == 0)
            Round.EndRound(RoundEndReason.NoVisitorsLeft);
    }

    private async Task UpdateStateAsync(bool roundActive, int roundNumber, Guid roundId, DateTime expiration,
        WordHint? wordHint = default)
    {
        State = new GameState
        {
            RoundActive = roundActive,
            RoundNumber = roundNumber,
            RoundId = roundId,
            IntervalEnd = expiration,
            WordHint = wordHint ?? WordHint.Default
        };

        await new GameStateChangedEvent(State).PublishAsync();
    }
}
