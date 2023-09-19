using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Integrations.GraphApi.Options;

public class GraphApiClientOptions
{
    public const string SectionName = "GraphApiClient";
    
    [Required] public string TenantId { get; set; } = string.Empty;
    [Required] public string ClientId { get; set; } = string.Empty;
    [Required] public string ClientSecret { get; set; } = string.Empty;
}
