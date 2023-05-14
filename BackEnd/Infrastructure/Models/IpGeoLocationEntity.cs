using Azure;
using Azure.Data.Tables;

namespace OhMyWord.Infrastructure.Models;

public record IpGeoLocationEntity : ITableEntity
{
    /// <summary>
    /// PartitionKey is IP version
    /// </summary>
    public string PartitionKey { get; set; } = string.Empty;

    /// <summary>
    /// RowKey is IP address
    /// </summary>
    public string RowKey { get; set; } = string.Empty;

    public string Country { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string FlagUrl { get; init; } = string.Empty;

    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
