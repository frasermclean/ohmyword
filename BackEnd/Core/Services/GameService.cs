using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Core.Events;
using OhMyWord.Core.Models;
using OhMyWord.Core.Options;
using OhMyWord.Data.Enums;

namespace OhMyWord.Core.Services;

public interface IGameService
{
    Round Round { get; }
    bool RoundActive { get; }
    DateTime Expiry { get; }
    GameServiceOptions Options { get; }

    event EventHandler<RoundStartedEventArgs> RoundStarted;
    event EventHandler<RoundEndedEventArgs> RoundEnded;
    event Action<LetterHint> LetterHintAdded;

    Task ExecuteGameAsync(CancellationToken gameCancellationToken);
    Task<int> ProcessGuessAsync(string connectionId, Guid roundId, string value);
}

public class GameService : IGameService
{
    private readonly ILogger<GameService> logger;
    private readonly IWordsService wordsService;
    private readonly IVisitorService visitorService;

    public Round Round { get; private set; } = Round.Default;
    public bool RoundActive { get; private set; }
    public DateTime Expiry { get; private set; }
    public GameServiceOptions Options { get; }

    public event Action<LetterHint>? LetterHintAdded;
    public event EventHandler<RoundStartedEventArgs>? RoundStarted;
    public event EventHandler<RoundEndedEventArgs>? RoundEnded;

    public GameService(ILogger<GameService> logger, IWordsService wordsService, IVisitorService visitorService,
        IOptions<GameServiceOptions> options)
    {
        this.logger = logger;
        this.wordsService = wordsService;
        this.visitorService = visitorService;

        Options = options.Value;

        visitorService.VisitorAdded += OnVisitorAdded;
        visitorService.VisitorRemoved += OnVisitorRemoved;
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
            RoundActive = true;
            RoundStarted?.Invoke(this, new RoundStartedEventArgs(Round));
            Expiry = Round.EndTime;

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
            RoundActive = false;
            Expiry = nextRoundStart;
            RoundEnded?.Invoke(this, new RoundEndedEventArgs(Round, nextRoundStart));
            Round.Dispose();

            logger.LogDebug("Round: {Number} has ended. Post round delay is: {Seconds} seconds", Round.Number,
                postRoundDelay.Seconds);

            await Task.Delay(postRoundDelay, gameCancellationToken);
        }
    }

    private async Task<Round> CreateRoundAsync(CancellationToken cancellationToken)
    {
        var word = await wordsService.GetNextWordAsync(cancellationToken);
        var duration = TimeSpan.FromSeconds(word.Length * Options.LetterHintDelay);
        var roundNumber = Round.Number + 1;

        return new Round(roundNumber, word, duration, visitorService.VisitorIds);
    }

    private async Task SendLetterHintsAsync(Round round)
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
            LetterHintAdded?.Invoke(letterHint);
        }
    }

    public async Task<int> ProcessGuessAsync(string connectionId, Guid roundId, string value)
    {
        var visitor = visitorService.GetVisitor(connectionId);

        // if round is not active then immediately return false
        if (!RoundActive || roundId != Round.Id) return 0;

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

    private void OnVisitorAdded(object? _, VisitorEventArgs args)
    {
        // visitor joined while round wasn't active
        if (!RoundActive)
            return;

        var wasAdded = Round.AddVisitor(args.VisitorId);
        if (!wasAdded)
            logger.LogError("Couldn't add visitor with ID {VisitorId} to round", args.VisitorId);
    }

    private void OnVisitorRemoved(object? _, VisitorEventArgs args)
    {
        // visitor left while round wasn't active
        if (!RoundActive)
            return;

        var wasRemoved = Round.RemoveVisitor(args.VisitorId);
        if (!wasRemoved)
            logger.LogError("Couldn't remove visitor with ID {VisitorId} from round", args.VisitorId);

        // last visitor left while round active
        if (args.VisitorCount == 0)
            Round.EndRound(RoundEndReason.NoVisitorsLeft);
    }
}
