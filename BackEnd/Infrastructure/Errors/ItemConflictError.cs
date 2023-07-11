using FluentResults;

namespace OhMyWord.Infrastructure.Errors;

public class ItemConflictError : Error
{
    public ItemConflictError(string id, string partition)
        : base($"Item with ID: {id} already exists on partition: {partition}")
    {
        Metadata.Add("Id", id);
        Metadata.Add("Partition", partition);
    }    
}
