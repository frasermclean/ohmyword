namespace OhMyWord.Data.CosmosDb.Models;

public sealed record SessionItem : GamesItem
{
    public int RoundCount { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }

    public override GamesItemCategory Category => GamesItemCategory.Session;
}
