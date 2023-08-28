namespace OhMyWord.Data.CosmosDb.Models;

public abstract record GamesItem : Item
{
    public abstract GamesItemCategory Category { get; }
}
