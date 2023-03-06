using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Infrastructure.Options;

public class CosmosDbOptions
{
    /// <summary>
    /// Section name for the app configuration.
    /// </summary>
    public const string SectionName = "CosmosDb";

    [Required] public string ConnectionString { get; init; } = string.Empty;
    [Required] public string DatabaseId { get; init; } = string.Empty;
    [Required] public IEnumerable<string> ContainerIds { get; init; } = Enumerable.Empty<string>();
    public string ApplicationName { get; init; } = "OhMyWord API";
}
