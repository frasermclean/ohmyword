using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Core.Models;
using OhMyWord.Services.Events;
using OhMyWord.Services.Models;
using OhMyWord.Services.Options;

namespace OhMyWord.Services.Game;

public interface IGameService
{
    Round Round { get; }
    bool RoundActive { get; }
    int RoundNumber { get; }
    DateTime Expiry { get; }

    event EventHandler<RoundStartedEventArgs> RoundStarted;
    event EventHandler<RoundEndedEventArgs> RoundEnded;
    event Action<LetterHint> LetterHintAdded;

    Task ExecuteGameAsync(CancellationToken gameCancellationToken);
    Task<int> ProcessGuessAsync(string playerId, string roundId, string value);
}

public class GameService : IGameService
{
    private readonly ILogger<GameService> logger;
    private readonly IWordsService wordsService;
    private readonly IPlayerService playerService;
    private readonly GameServiceOptions options;

    public Round Round { get; private set; } = Round.Default;
    public int RoundNumber { get; private set; }
    public bool RoundActive { get; private set; }
    public DateTime Expiry { get; private set; }

    public event Action<LetterHint>? LetterHintAdded;
    public event EventHandler<RoundStartedEventArgs>? RoundStarted;
    public event EventHandler<RoundEndedEventArgs>? RoundEnded;

    public GameService(ILogger<GameService> logger, IWordsService wordsService, IPlayerService playerService, IOptions<GameServiceOptions> options)
    {
        this.logger = logger;
        this.wordsService = wordsService;
        this.playerService = playerService;
        this.options = options.Value;

        playerService.PlayerRemoved += OnPlayerRemoved;
    }

    public async Task ExecuteGameAsync(CancellationToken gameCancellationToken)
    {
        while (!gameCancellationToken.IsCancellationRequested)
        {
            // sleep while there are no players
            if (playerService.PlayerCount == 0)
            {
                await Task.Delay(1000, gameCancellationToken);
                continue;
            }

            // start new round
            var word = await wordsService.SelectRandomWordAsync(gameCancellationToken);
            var duration = TimeSpan.FromSeconds(word.Value.Length * options.LetterHintDelay);
            RoundActive = true;
            Round = new Round(++RoundNumber, word, duration);
            RoundStarted?.Invoke(this, new RoundStartedEventArgs
            {
                RoundId = Round.Id,
                RoundNumber = RoundNumber,
                RoundEnds = Round.EndTime,
                WordHint = Round.WordHint,
            });
            Expiry = Round.EndTime;

            logger.LogDebug("Round: {roundNumber} has started. Current currentWord is: {word}. Round duration: {seconds} seconds.",
                Round.Number, Round.Word, Round.Duration.Seconds);

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
            var postRoundDelay = TimeSpan.FromSeconds(options.PostRoundDelay);
            var nextRoundStart = DateTime.UtcNow + postRoundDelay;
            RoundActive = false;
            RoundEnded?.Invoke(this, new RoundEndedEventArgs
            {
                Round = Round,
                NextRoundStart = nextRoundStart,
            });
            Expiry = nextRoundStart;
            Round.Dispose();
            Round = Round.Default;

            logger.LogDebug("Round: {number} has ended. Post round delay is: {seconds} seconds", RoundNumber, postRoundDelay.Seconds);

            await Task.Delay(postRoundDelay, gameCancellationToken);
        }
    }

    private async Task SendLetterHintsAsync(Round round)
    {
        var word = round.Word;
        var wordHint = round.WordHint;
        var letterDelay = round.Duration / word.Value.Length;
        var previousIndices = new List<int>();

        while (previousIndices.Count < word.Value.Length && !round.CancellationToken.IsCancellationRequested)
        {
            await Task.Delay(letterDelay, round.CancellationToken);

            int index;
            do index = Random.Shared.Next(word.Value.Length);
            while (previousIndices.Contains(index));
            previousIndices.Add(index);

            var letterHint = new LetterHint
            {
                Position = index + 1,
                Value = word.Value[index]
            };

            logger.LogDebug("Added letter hint. Position: {position}, value: {value}",
                letterHint.Position, letterHint.Value);

            LetterHintAdded?.Invoke(letterHint);
            wordHint.AddLetterHint(letterHint);
        }
    }

    public async Task<int> ProcessGuessAsync(string playerId, string roundId, string value)
    {
        // if round is not active then immediately return false
        if (Round is null || roundId != Round.Id.ToString()) return 0;

        // compare value to current word value
        if (!string.Equals(value, Round.Word.Value, StringComparison.InvariantCultureIgnoreCase))
            return 0;

        const int points = 100; // TODO: Calculate points value dynamically
        var isSuccessful = await playerService.AwardPlayerPointsAsync(playerId, points);

        // end round if all players have been awarded points
        if (playerService.AllPlayersAwarded)
            Round.EndRound(RoundEndReason.AllPlayersAwarded);

        return isSuccessful ? points : 0;
    }

    private void OnPlayerRemoved(object? _, PlayerEventArgs args)
    {
        if (!RoundActive || args.PlayerCount > 0)
            return;

        // end round early if no players left
        Round.EndRound(RoundEndReason.NoPlayersLeft);
    }
}
