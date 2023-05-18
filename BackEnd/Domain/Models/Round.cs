using OhMyWord.Domain.Options;
using OhMyWord.Infrastructure.Models.Entities;
using System.Collections.Concurrent;

namespace OhMyWord.Domain.Models;

public sealed class Round : IDisposable
{
    private readonly ConcurrentDictionary<string, RoundPlayerData> playerData;
    private readonly CancellationTokenSource cancellationTokenSource = new();

    internal Round(Word word, RoundOptions? options = default, IEnumerable<string>? playerIds = default)
    {
        Word = word;
        WordHint = new WordHint(word);
        GuessLimit = options?.GuessLimit ?? RoundOptions.GuessLimitDefault;
        StartDate = DateTime.UtcNow;
        EndDate = StartDate + word.Length *
            TimeSpan.FromSeconds(options?.LetterHintDelay ?? RoundOptions.LetterHintDelayDefault);

        playerData = playerIds is null
            ? new ConcurrentDictionary<string, RoundPlayerData>()
            : new ConcurrentDictionary<string, RoundPlayerData>(playerIds.Select(playerId =>
                new KeyValuePair<string, RoundPlayerData>(playerId, new RoundPlayerData(playerId))));
    }

    public Guid Id { get; init; }
    public int Number { get; init; }
    public Word Word { get; }
    public WordHint WordHint { get; }
    public int GuessLimit { get; }
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public RoundEndReason EndReason { get; private set; }
    public required Guid SessionId { get; init; }
    public CancellationToken CancellationToken => cancellationTokenSource.Token;
    public int PlayerCount => playerData.Count;
    public bool AllPlayersGuessed => !playerData.IsEmpty && playerData.Values.All(player => player.PointsAwarded > 0);

    public bool AddPlayer(string playerId)
        => playerData.TryAdd(playerId, new RoundPlayerData(playerId));

    public void EndRound(RoundEndReason endReason)
    {
        EndReason = endReason;
        cancellationTokenSource.Cancel();
    }

    public bool IncrementGuessCount(string playerId)
    {
        if (!playerData.TryGetValue(playerId, out var data))
            return false;

        if (data.GuessCount >= GuessLimit)
            return false;

        data.GuessCount++;
        return true;
    }

    public bool AwardPoints(string playerId, int points)
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

    public static Round Default => new(Word.Default) { SessionId = Session.Default.Id };
}
