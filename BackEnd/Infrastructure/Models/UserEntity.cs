using Azure;
using Azure.Data.Tables;

namespace OhMyWord.Infrastructure.Models;

public record UserEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public Role Role { get; init; } = Role.User;

    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
