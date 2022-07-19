﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Core.Events;
using OhMyWord.Core.Options;
using OhMyWord.Data.Models;

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
    private readonly IPlayerService playerService;

    public Round Round { get; private set; } = Round.Default;
    public bool RoundActive { get; private set; }
    public DateTime Expiry { get; private set; }
    public GameServiceOptions Options { get; }

    public event Action<LetterHint>? LetterHintAdded;
    public event EventHandler<RoundStartedEventArgs>? RoundStarted;
    public event EventHandler<RoundEndedEventArgs>? RoundEnded;

    public GameService(ILogger<GameService> logger, IWordsService wordsService, IPlayerService playerService, IOptions<GameServiceOptions> options)
    {
        this.logger = logger;
        this.wordsService = wordsService;
        this.playerService = playerService;

        Options = options.Value;

        playerService.PlayerAdded += OnPlayerAdded;
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
            Round = await CreateRoundAsync(gameCancellationToken);
            RoundActive = true;
            RoundStarted?.Invoke(this, new RoundStartedEventArgs(Round));
            Expiry = Round.EndTime;

            logger.LogDebug("Round: {roundNumber} has started with {playerCount} players. Current word is: {word}. Round duration: {seconds} seconds.",
                Round.Number, Round.PlayerCount, Round.Word, Round.Duration.Seconds);

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

            logger.LogDebug("Round: {number} has ended. Post round delay is: {seconds} seconds", Round.Number, postRoundDelay.Seconds);

            await Task.Delay(postRoundDelay, gameCancellationToken);
        }
    }

    private async Task<Round> CreateRoundAsync(CancellationToken cancellationToken)
    {
        var word = await wordsService.GetNextWordAsync(cancellationToken);
        var duration = TimeSpan.FromSeconds(word.Value.Length * Options.LetterHintDelay);
        var roundNumber = Round.Number + 1;

        return new Round(roundNumber, word, duration, playerService.PlayerIds);
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

            var letterHint = word.GetLetterHint(index + 1);

            wordHint.AddLetterHint(letterHint);
            LetterHintAdded?.Invoke(letterHint);
        }
    }

    public async Task<int> ProcessGuessAsync(string connectionId, Guid roundId, string value)
    {
        var player = playerService.GetPlayer(connectionId);

        // if round is not active then immediately return false
        if (!RoundActive || roundId != Round.Id) return 0;

        // compare value to current word value
        if (!string.Equals(value, Round.Word.Value, StringComparison.InvariantCultureIgnoreCase))
            return 0;

        var guessCountIncremented = Round.IncrementGuessCount(player.Id);
        if (!guessCountIncremented)
            logger.LogWarning("Couldn't increment guess count of player with ID: {playerId}", player.Id);

        var pointsAwarded = Round.AwardPlayer(player.Id);
        if (pointsAwarded == 0)
            logger.LogWarning("Zero points were awarded to player with ID: {playerId}", player.Id);

        await playerService.IncrementPlayerScoreAsync(player.Id, pointsAwarded);

        // end round if all players have been awarded points
        if (Round.AllPlayersAwarded)
            Round.EndRound(RoundEndReason.AllPlayersAwarded);

        return pointsAwarded;
    }

    private void OnPlayerAdded(object? _, PlayerEventArgs args)
    {
        // player joined while round wasn't active
        if (!RoundActive)
            return;

        var wasAdded = Round.AddPlayer(args.PlayerId);
        if (!wasAdded)
            logger.LogError("Couldn't add player with ID {playerId} to round.", args.PlayerId);
    }

    private void OnPlayerRemoved(object? _, PlayerEventArgs args)
    {
        // player left while round wasn't active
        if (!RoundActive)
            return;

        var wasRemoved = Round.RemovePlayer(args.PlayerId);
        if (!wasRemoved)
            logger.LogError("Couldn't remove player with ID {playerId} from round.", args.PlayerId);

        // last player left while round active
        if (args.PlayerCount == 0)
            Round.EndRound(RoundEndReason.NoPlayersLeft);
    }
}