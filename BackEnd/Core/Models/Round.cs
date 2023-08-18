using System.Collections.Concurrent;

namespace OhMyWord.Core.Models;

public sealed class Round
{
    public Guid Id { get; } = Guid.NewGuid();
    public required int Number { get; init; }
    public required Word Word { get; init; }
    public required WordHint WordHint { get; init; }
    public required int GuessLimit { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; set; }
    public required Guid SessionId { get; init; }
    public RoundEndReason? EndReason { get; set; }

    public IDictionary<Guid, RoundPlayerData> PlayerData { get; } = new ConcurrentDictionary<Guid, RoundPlayerData>();

    /// <summary>
    /// True if all players have been awarded points for this round.
    /// </summary>
    public bool AllPlayersAwarded => PlayerData.Count > 0 && PlayerData.Values.All(player => player.PointsAwarded > 0);

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
