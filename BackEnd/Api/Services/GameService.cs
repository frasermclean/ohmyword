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

    Task ExecuteGameAsync(CancellationToken gameCancellationToken);
    Task<int> ProcessGuessAsync(string connectionId, Guid roundId, string value);
    void AddPlayer(Player player);
    void RemovePlayer(string connectionId);
}

public class GameService : IGameService
{
    private readonly ILogger<GameService> logger;
    private readonly IWordsService wordsService;
    private readonly IPlayerService playerService;

    private IReadOnlyList<string> allWordIds = new List<string>();
    private Stack<string> shuffledWordIds = new();

    public GameState State { get; private set; } = new();
    public Round Round { get; private set; } = Round.Default;
    public GameServiceOptions Options { get; }

    public GameService(ILogger<GameService> logger, IWordsService wordsService, IPlayerService playerService,
        IOptions<GameServiceOptions> options)
    {
        this.logger = logger;
        this.wordsService = wordsService;
        this.playerService = playerService;

        Options = options.Value;
    }

    public async Task ExecuteGameAsync(CancellationToken gameCancellationToken)
    {
        while (!gameCancellationToken.IsCancellationRequested)
        {
            // sleep while there are no visitors
            if (playerService.PlayerCount == 0)
            {
                await Task.Delay(1000, gameCancellationToken);
                continue;
            }

            // start new round
            Round = await CreateRoundAsync(gameCancellationToken);
            await UpdateStateAsync(true, Round, Round.EndTime);

            logger.LogDebug(
                "Round: {RoundNumber} has started with {PlayerCount} visitors. Current word is: {WordId}. Round duration: {Seconds} seconds",
                Round.Number, Round.PlayerCount, Round.Word.Id, Round.Duration.Seconds);

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
            await UpdateStateAsync(false, Round, DateTime.UtcNow + postRoundDelay);

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

        return new Round(roundNumber, word, duration, playerService.PlayerIds);
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
        var player = playerService.GetPlayer(connectionId);

        // if round is not active then immediately return false
        if (!State.RoundActive || roundId != Round.Id) return 0;

        // compare value to current word value
        if (!string.Equals(value, Round.Word.Id, StringComparison.InvariantCultureIgnoreCase))
            return 0;

        var guessCountIncremented = Round.IncrementGuessCount(player.Id);
        if (!guessCountIncremented)
            logger.LogWarning("Couldn't increment guess count of player with ID: {PlayerId}", player.Id);

        const int pointsToAward = 100; // TODO: Calculate points dynamically
        var pointsAwarded = Round.AwardVisitor(player.Id, pointsToAward);
        if (!pointsAwarded)
            logger.LogWarning("Zero points were awarded to player with ID: {PlayerId}", player.Id);

        await playerService.IncrementPlayerScoreAsync(player.Id,
            pointsToAward); // TODO: Write to database after round end

        // end round if all players have been awarded points
        if (Round.AllPlayersAwarded)
            Round.EndRound(RoundEndReason.AllPlayersAwarded);

        return pointsToAward;
    }

    public void AddPlayer(Player player)
    {
        // player joined while round wasn't active
        if (!State.RoundActive)
            return;

        Round.AddPlayer(player.Id);
    }

    public void RemovePlayer(string connectionId)
    {
        // player left while round wasn't active
        if (!State.RoundActive)
            return;

        // last visitor left while round active
        if (playerService.PlayerCount == 0)
            Round.EndRound(RoundEndReason.NoPlayersLeft);
    }

    private async Task UpdateStateAsync(bool roundActive, Round round, DateTime intervalEnd)
    {
        State = new GameState
        {
            RoundActive = roundActive,
            RoundNumber = round.Number,
            RoundId = round.Id,
            IntervalEnd = intervalEnd,
            WordHint = round.WordHint,
            RoundSummary = !roundActive
                ? new RoundSummary
                {
                    Word = round.Word.Id, PartOfSpeech = round.WordHint.PartOfSpeech, EndReason = round.EndReason,
                }
                : default
        };

        await new GameStateChangedEvent(State).PublishAsync();
    }
}
