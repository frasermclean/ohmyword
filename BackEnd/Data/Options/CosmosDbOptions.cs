using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Data.Options;

public class CosmosDbOptions
{
    /// <summary>
    /// Section name for the app configuration.
    /// </summary>
    public const string SectionName = "CosmosDb";

    [Required] public string ConnectionString { get; init; } = string.Empty;
    public IEnumerable<DatabaseDefinition> Databases { get; init; } = Enumerable.Empty<DatabaseDefinition>();
    public string ApplicationName { get; init; } = "OhMyWord API";
}
