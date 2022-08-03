using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Data.Options;

public class CosmosDbOptions
{
    /// <summary>
    /// Section name for the app configuration.
    /// </summary>
    public const string SectionName = "CosmosDb";
    
    [Required] public string DatabaseId { get; set; } = string.Empty;
    [Required] public string ConnectionString { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public string ApplicationName { get; init; } = "OhMyWord API";
}
