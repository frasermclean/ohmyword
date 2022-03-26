namespace OhMyWord.Data.Models;

public abstract class Entity
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

    public string GetPartition() => Id;
}
