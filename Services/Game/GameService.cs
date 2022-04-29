﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Core.Models;
using OhMyWord.Services.Models;
using OhMyWord.Services.Models.Events;
using OhMyWord.Services.Options;

namespace OhMyWord.Services.Game;

public interface IGameService
{
    Round? Round { get; }
    bool RoundActive { get; }
    int RoundNumber { get; }
    DateTime Expiry { get; }

    event Action<RoundStart> RoundStarted;
    event Action<RoundEnd> RoundEnded;
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

    public Round? Round { get; private set; }
    public int RoundNumber { get; private set; }
    public bool RoundActive => Round is not null;
    public DateTime Expiry { get; private set; }

    public event Action<LetterHint>? LetterHintAdded;
    public event Action<RoundStart>? RoundStarted;
    public event Action<RoundEnd>? RoundEnded;

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

            Round = await StartRoundAsync(++RoundNumber, gameCancellationToken);

            try
            {
                await SendLetterHintsAsync(Round);
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation("Round has been terminated early.");
            }

            await EndRoundAsync(Round, gameCancellationToken);
        }
    }

    private async Task<Round> StartRoundAsync(int roundNumber, CancellationToken cancellationToken)
    {
        var word = await wordsService.SelectRandomWordAsync(cancellationToken); 
        var duration = TimeSpan.FromSeconds(word.Value.Length * options.LetterHintDelay);

        var round = new Round(roundNumber, word, duration);
        Expiry = round.Expiry;
        RoundStarted?.Invoke(new RoundStart(round));

        logger.LogDebug("Round: {roundNumber} has started. Current currentWord is: {word}. Round duration: {seconds} seconds.",
            round.Number, round.Word, duration.Seconds);

        return round;
    }

    private async Task EndRoundAsync(Round round, CancellationToken cancellationToken)
    {
        var postRoundDelay = TimeSpan.FromSeconds(options.PostRoundDelay);
        Expiry = DateTime.UtcNow + postRoundDelay;
        round.Dispose();
        Round = null;
        logger.LogDebug("Round: {number} has ended. Post round delay is: {seconds} seconds", round.Number, postRoundDelay.Seconds);
        RoundEnded?.Invoke(new RoundEnd(round, DateTime.UtcNow + postRoundDelay));
        await Task.Delay(postRoundDelay, cancellationToken);
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
        if (Round is null || args.PlayerCount > 0)
            return;
        
        // end round early if no players left
        Round.EndRound(RoundEndReason.NoPlayersLeft);
    }
}
