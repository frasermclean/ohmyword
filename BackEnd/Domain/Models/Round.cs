using OhMyWord.Infrastructure.Models.Entities;
using System.Collections.Concurrent;

namespace OhMyWord.Domain.Models;

public sealed class Round : IDisposable
{
    private readonly ConcurrentDictionary<string, RoundPlayerData> playerData;
    private readonly CancellationTokenSource cancellationTokenSource = new();

    internal Round(Word word, int roundNumber, IEnumerable<string> playerIds, double letterHintDelay, int guessLimit)
    {
        playerData = new ConcurrentDictionary<string, RoundPlayerData>(playerIds.Select(id =>
            new KeyValuePair<string, RoundPlayerData>(id, new RoundPlayerData())));

        RoundNumber = roundNumber;
        RoundId = Guid.NewGuid();
        Word = word;
        WordHint = new WordHint(word);
        GuessLimit = guessLimit;
        StartDate = DateTime.UtcNow;
        EndDate = StartDate + word.Length * TimeSpan.FromSeconds(letterHintDelay);
    }

    public int RoundNumber { get; }
    public Guid RoundId { get; }
    public Word Word { get; }
    public WordHint WordHint { get; }
    public int GuessLimit { get; }
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public RoundEndReason EndReason { get; private set; }
    public CancellationToken CancellationToken => cancellationTokenSource.Token;
    public bool AllPlayersGuessed => playerData.Values.All(player => player.PointsAwarded > 0);

    public bool AddPlayer(string playerId)
        => playerData.TryAdd(playerId, new RoundPlayerData());

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
        return true;
    }

    public void Dispose()
    {
        cancellationTokenSource.Dispose();
    }
}
