using Microsoft.Extensions.Options;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Options;
using OhMyWord.Infrastructure.Models.Entities;

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
    /// Starts a new round.
    /// </summary>
    /// <param name="word">The word to use for the round.</param>
    /// <returns>Data pertaining to the started round.</returns>
    (RoundStartData, CancellationToken) StartRound(Word word);

    /// <summary>
    /// End the current round.
    /// </summary>
    /// <param name="endReason">The reason for ending the round.</param>
    /// <returns></returns>
    RoundEndData EndRound(RoundEndReason endReason = RoundEndReason.Timeout);
}

public sealed class RoundService : IRoundService
{
    private readonly TimeSpan letterHintDelay;
    private readonly TimeSpan postRoundDelay;

    private CancellationTokenSource? cancellationTokenSource;

    public RoundService(IOptions<RoundServiceOptions> options)
    {
        letterHintDelay = TimeSpan.FromSeconds(options.Value.LetterHintDelay);
        postRoundDelay = TimeSpan.FromSeconds(options.Value.PostRoundDelay);
    }

    public bool IsRoundActive { get; private set; }
    public int RoundNumber { get; private set; }
    public Guid RoundId { get; private set; }
    public Word Word { get; private set; } = Word.Default;
    public WordHint WordHint { get; private set; } = WordHint.Default;
    public DateTime IntervalStart { get; private set; }
    public DateTime IntervalEnd { get; private set; }
    public RoundEndReason LastEndReason { get; private set; }

    public (RoundStartData, CancellationToken) StartRound(Word word)
    {
        IsRoundActive = true;
        RoundNumber++;
        RoundId = Guid.NewGuid();
        Word = word;
        WordHint = new WordHint(word);
        IntervalStart = DateTime.UtcNow;
        IntervalEnd = IntervalStart + word.Length * letterHintDelay;
        cancellationTokenSource = new CancellationTokenSource();

        var data = new RoundStartData
        {
            RoundNumber = RoundNumber,
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

        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = null;

        return new RoundEndData
        {
            RoundId = RoundId,
            Word = Word.Id,
            EndReason = endReason,
            PostRoundDelay = postRoundDelay,
            NextRoundStart = DateTime.UtcNow + postRoundDelay
        };
    }
}
