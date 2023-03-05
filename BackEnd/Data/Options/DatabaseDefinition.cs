namespace OhMyWord.Data.Options;

public class DatabaseDefinition
{
    public string DatabaseId { get; init; } = string.Empty;
    public IEnumerable<string> ContainerIds { get; init; } = Enumerable.Empty<string>();
}
