using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using OhMyWord.Infrastructure.Models;

namespace OhMyWord.Infrastructure.Services;

public interface IUsersRepository
{
    IAsyncEnumerable<UserEntity> SearchUsers(string? filter = default, CancellationToken cancellationToken = default);
    Task<UserEntity?> GetUserAsync(string userId, CancellationToken cancellationToken = default);
    Task UpsertUserAsync(UserEntity userEntity, CancellationToken cancellationToken = default);
}

public class UsersRepository : IUsersRepository
{
    private readonly ILogger<UsersRepository> logger;
    private readonly TableClient tableClient;

    public UsersRepository(ILogger<UsersRepository> logger, TableServiceClient serviceClient)
    {
        this.logger = logger;
        tableClient = serviceClient.GetTableClient("users");
    }

    public IAsyncEnumerable<UserEntity> SearchUsers(string? filter = default,
        CancellationToken cancellationToken = default)
    {
        return filter is null
            ? tableClient.QueryAsync<UserEntity>(cancellationToken: cancellationToken)
            : tableClient.QueryAsync<UserEntity>(entity =>
                    entity.PartitionKey.Contains(filter, StringComparison.InvariantCultureIgnoreCase) ||
                    entity.RowKey.Contains(filter, StringComparison.InvariantCultureIgnoreCase) ||
                    entity.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase) ||
                    entity.Email.Contains(filter, StringComparison.InvariantCultureIgnoreCase),
                cancellationToken: cancellationToken);
    }

    public async Task<UserEntity?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await tableClient
            .QueryAsync<UserEntity>(entity => entity.RowKey == userId, cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            logger.LogWarning("User with ID: {UserId} was not found", userId);

        return user;
    }

    public async Task UpsertUserAsync(UserEntity userEntity, CancellationToken cancellationToken = default)
    {
        await tableClient.UpsertEntityAsync(userEntity, cancellationToken: cancellationToken);
    }
}
