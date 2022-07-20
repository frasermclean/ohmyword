using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Data.Options;

public class CosmosDbOptions
{
    [Required] public string DatabaseId { get; set; } = string.Empty;
    [Required] public string ConnectionString { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public string ApplicationName { get; init; } = "OhMyWord API";
}