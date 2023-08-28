using OhMyWord.Core.Models;

namespace OhMyWord.Data.CosmosDb.Models;

public sealed record RoundItem : GamesItem
{
    public int Number { get; init; }
    public string WordId { get; init; } = string.Empty;
    public Guid DefinitionId { get; init; }
    public int GuessLimit { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public RoundEndReason EndReason { get; init; }
    public Guid SessionId { get; init; }
    public IEnumerable<RoundPlayerData> PlayerData { get; init; } = Enumerable.Empty<RoundPlayerData>();

    public override GamesItemCategory Category => GamesItemCategory.Round;

    public override string GetPartition() => SessionId.ToString();
}
