using Microsoft.Extensions.Options;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Options;
using OhMyWord.Infrastructure.Models.Entities;
using System.Collections.Concurrent;

namespace OhMyWord.Domain.Services;

public interface IRoundService
{
    /// <summary>
    /// True if a round is currently active.
    /// </summary>
    bool IsRoundActive { get; }

    /// <summary>
    /// Current round number.
    /// </summary>
    int RoundNumber { get; }

    /// <summary>
    /// Current round unique identifier.
    /// </summary>
    Guid RoundId { get; }

    /// <summary>
    /// <see cref="Word"/> for the current round.
    /// </summary>
    Word Word { get; }

    /// <summary>
    /// <see cref="WordHint"/> for the current round.
    /// </summary>
    WordHint WordHint { get; }

    /// <summary>
    /// The time when the current interval started.
    /// </summary>
    DateTime IntervalStart { get; }

    /// <summary>
    /// The time when the current interval ends.
    /// </summary>
    DateTime IntervalEnd { get; }

    /// <summary>
    /// The reason the last round ended.
    /// </summary>
    RoundEndReason LastEndReason { get; }

    /// <summary>
    /// True if all players have guessed the word.
    /// </summary>
    bool AllPlayersGuessed { get; }

    /// <summary>
    /// Starts a new round.
    /// </summary>
    /// <param name="word">The word to use for the round.</param>
    /// <param name="roundNumber">The round number to create the round with.</param>
    /// <returns>Data pertaining to the started round.</returns>
    (RoundStartData, CancellationToken) StartRound(Word word, int roundNumber);

    /// <summary>
    /// End the current round.
    /// </summary>
    /// <param name="endReason">The reason for ending the round.</param>
    /// <returns></returns>
    RoundEndData EndRound(RoundEndReason endReason = RoundEndReason.Timeout);

    /// <summary>
    /// Add a player to the current round.
    /// </summary>
    /// <param name="playerId">The ID of the <see cref="Player"/> to add.</param>
    /// <returns>True if successful, false on failure.</returns>
    bool AddPlayer(string playerId);

    bool IncrementGuessCount(string playerId);
    bool AwardPoints(string playerId, int points);
}

public sealed class RoundService : IRoundService, IDisposable
{
    private readonly IPlayerService playerService;
    private readonly TimeSpan letterHintDelay;
    private readonly TimeSpan postRoundDelay;
    private readonly int guessLimit;
    private readonly ConcurrentDictionary<string, RoundPlayerData> playerData = new();
    private readonly CancellationTokenSource cancellationTokenSource = new();

    public RoundService(IOptions<RoundOptions> options, IPlayerService playerService)
    {
        this.playerService = playerService;
        letterHintDelay = TimeSpan.FromSeconds(options.Value.LetterHintDelay);
        postRoundDelay = TimeSpan.FromSeconds(options.Value.PostRoundDelay);
        guessLimit = options.Value.GuessLimit;
    }

    public bool IsRoundActive { get; private set; }
    public int RoundNumber { get; private set; }
    public Guid RoundId { get; private set; }
    public Word Word { get; private set; } = Word.Default;
    public WordHint WordHint { get; private set; } = WordHint.Default;
    public DateTime IntervalStart { get; private set; }
    public DateTime IntervalEnd { get; private set; }
    public RoundEndReason LastEndReason { get; private set; }
    public bool AllPlayersGuessed => playerData.Values.All(player => player.PointsAwarded > 0);

    public (RoundStartData, CancellationToken) StartRound(Word word, int roundNumber)
    {
        IsRoundActive = true;
        RoundNumber = roundNumber;
        RoundId = Guid.NewGuid();
        Word = word;
        WordHint = new WordHint(word);
        IntervalStart = DateTime.UtcNow;
        IntervalEnd = IntervalStart + word.Length * letterHintDelay;

        // add currently connected players to the round
        foreach (var playerId in playerService.PlayerIds)
            AddPlayer(playerId);

        var data = new RoundStartData
        {
            RoundNumber = roundNumber,
            RoundId = RoundId,
            WordHint = WordHint,
            StartDate = IntervalStart,
            EndDate = IntervalEnd,
        };

        return (data, cancellationTokenSource.Token);
    }

    public RoundEndData EndRound(RoundEndReason endReason)
    {
        IsRoundActive = false;
        LastEndReason = endReason;
        cancellationTokenSource.Cancel();

        return new RoundEndData
        {
            RoundId = RoundId,
            Word = Word.Id,
            EndReason = endReason,
            PostRoundDelay = postRoundDelay,
            NextRoundStart = DateTime.UtcNow + postRoundDelay
        };
    }

    public bool AddPlayer(string playerId)
        => IsRoundActive && playerData.TryAdd(playerId, new RoundPlayerData());

    public bool IncrementGuessCount(string playerId)
    {
        if (!playerData.TryGetValue(playerId, out var data))
            return false;

        if (data.GuessCount >= guessLimit)
            return false;

        data.GuessCount++;
        return true;
    }

    public bool AwardPoints(string playerId, int points)
    {
        if (!playerData.TryGetValue(playerId, out var data))
            return false;

        data.PointsAwarded = points;
        return true;
    }

    public void Dispose()
    {
        cancellationTokenSource.Dispose();
    }
}
