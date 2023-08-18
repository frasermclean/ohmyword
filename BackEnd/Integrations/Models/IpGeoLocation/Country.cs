namespace OhMyWord.Integrations.Models.IpGeoLocation;

internal class Country
{
    public string? Name { get; set; } = string.Empty;
    public string? Code { get; set; } = string.Empty;    
    public Flag Flag { get; set; } = new();
}
