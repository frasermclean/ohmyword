namespace OhMyWord.Data.CosmosDb.Models;

public abstract record UserCreatedItem : Item
{
    public Guid LastModifiedBy { get; init; }
}
