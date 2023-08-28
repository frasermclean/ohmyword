using FluentResults;

namespace OhMyWord.Data.CosmosDb.Errors;

public class ItemNotFoundError : Error
{
    public ItemNotFoundError(string id, string partition)
        : base($"Item with ID: {id} was not found on partition: {partition}")
    {
        Metadata.Add("Id", id);
        Metadata.Add("Partition", partition);
    }
}
