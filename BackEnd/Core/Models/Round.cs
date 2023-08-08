using System.Collections.Concurrent;

namespace OhMyWord.Core.Models;

public sealed class Round
{
    public Round(IEnumerable<Guid>? playerIds = default)
    {
        PlayerData = new ConcurrentDictionary<Guid, RoundPlayerData>(
            playerIds?.Select(id => new KeyValuePair<Guid, RoundPlayerData>(id, new RoundPlayerData(id))) ??
            Enumerable.Empty<KeyValuePair<Guid, RoundPlayerData>>());
    }

    public Guid Id { get; } = Guid.NewGuid();
    public required int Number { get; init; }
    public required Word Word { get; init; }
    public required WordHint WordHint { get; init; }
    public required int GuessLimit { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; set; }
    public required Guid SessionId { get; init; }
    public RoundEndReason? EndReason { get; set; }
    public ConcurrentDictionary<Guid, RoundPlayerData> PlayerData { get; }

    /// <summary>
    /// True if all players have been awarded points for this round.
    /// </summary>
    public bool AllPlayersAwarded => !PlayerData.IsEmpty && PlayerData.Values.All(player => player.PointsAwarded > 0);

    public bool IsActive => EndReason is null && EndDate > DateTime.UtcNow;

    public static readonly Round Default = new()
    {
        Number = default,
        Word = Word.Default,
        WordHint = WordHint.Default,
        GuessLimit = default,
        StartDate = default,
        EndDate = default,
        SessionId = Guid.Empty
    };
}
