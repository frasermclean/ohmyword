using OhMyWord.Domain.Options;
using OhMyWord.Infrastructure.Models.Entities;
using System.Collections.Concurrent;

namespace OhMyWord.Domain.Models;

public sealed class Round : IDisposable
{
    private readonly ConcurrentDictionary<Guid, RoundPlayerData> playerData;
    private readonly CancellationTokenSource cancellationTokenSource = new();

    internal Round(Word word, double letterHintDelay, IEnumerable<Guid>? playerIds = default)
    {
        Word = word;
        WordHint = new WordHint(word);
        EndDate = StartDate + word.Length * TimeSpan.FromSeconds(letterHintDelay);

        playerData = playerIds is null
            ? new ConcurrentDictionary<Guid, RoundPlayerData>()
            : new ConcurrentDictionary<Guid, RoundPlayerData>(playerIds.Select(playerId =>
                new KeyValuePair<Guid, RoundPlayerData>(playerId, new RoundPlayerData(playerId))));
    }

    public Guid Id { get; private init; } = Guid.NewGuid();
    public required int Number { get; init; }
    public Word Word { get; }
    public WordHint WordHint { get; }
    public required int GuessLimit { get; init; }
    public DateTime StartDate { get; } = DateTime.UtcNow;
    public DateTime EndDate { get; }
    public RoundEndReason? EndReason { get; private set; }
    public required Guid SessionId { get; init; }
    public CancellationToken CancellationToken => cancellationTokenSource.Token;
    public int PlayerCount => playerData.Count;
    public bool AllPlayersGuessed => !playerData.IsEmpty && playerData.Values.All(player => player.PointsAwarded > 0);

    public bool IsActive => !cancellationTokenSource.IsCancellationRequested &&
                            DateTime.UtcNow > StartDate &&
                            DateTime.UtcNow < EndDate;

    public bool AddPlayer(Guid playerId)
        => playerData.TryAdd(playerId, new RoundPlayerData(playerId));

    public void EndRound(RoundEndReason endReason)
    {
        EndReason = endReason;
        cancellationTokenSource.Cancel();
    }

    public bool IncrementGuessCount(Guid playerId)
    {
        if (!playerData.TryGetValue(playerId, out var data))
            return false;

        if (data.GuessCount >= GuessLimit)
            return false;

        data.GuessCount++;
        return true;
    }

    public bool AwardPoints(Guid playerId, int points)
    {
        if (!playerData.TryGetValue(playerId, out var data))
            return false;

        data.PointsAwarded = points;
        data.GuessTime = DateTime.UtcNow - StartDate;

        return true;
    }

    public IEnumerable<RoundPlayerData> GetPlayerData() => playerData.Values;

    public void Dispose()
    {
        cancellationTokenSource.Dispose();
    }

    public static Round Default => new(Word.Default, RoundOptions.LetterHintDelayDefault)
    {
        Id = Guid.Empty,
        Number = default,
        SessionId = Guid.Empty,
        GuessLimit = RoundOptions.GuessLimitDefault
    };
}
