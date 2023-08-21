using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using OhMyWord.Data.Tables.Models;

namespace OhMyWord.Data.Tables.Services;

public class UsersRepository : IUsersRepository
{
    private readonly ILogger<UsersRepository> logger;
    private readonly TableClient tableClient;

    public UsersRepository(ILogger<UsersRepository> logger, TableServiceClient serviceClient)
    {
        this.logger = logger;
        tableClient = serviceClient.GetTableClient("users");
    }

    public IAsyncEnumerable<User> SearchUsers(string? filter = default,
        CancellationToken cancellationToken = default)
    {
        var queryResult = filter is null
            ? tableClient.QueryAsync<UserEntity>(cancellationToken: cancellationToken)
            : tableClient.QueryAsync<UserEntity>(entity =>
                    entity.PartitionKey.Contains(filter, StringComparison.InvariantCultureIgnoreCase) ||
                    entity.RowKey.Contains(filter, StringComparison.InvariantCultureIgnoreCase) ||
                    entity.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase) ||
                    entity.Email.Contains(filter, StringComparison.InvariantCultureIgnoreCase),
                cancellationToken: cancellationToken);

        return queryResult.Select(MapToUser);
    }

    public async Task<User?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var userEntity = await tableClient
            .QueryAsync<UserEntity>(entity => entity.RowKey == userId, cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (userEntity is not null)
            return MapToUser(userEntity);

        logger.LogWarning("User with ID: {UserId} was not found", userId);
        return default;
    }

    public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            var userEntity = MapToEntity(user);
            await tableClient.AddEntityAsync(userEntity, cancellationToken: cancellationToken);
            logger.LogInformation("User with ID: {UserId} was added", user.Id);
        }
        catch (RequestFailedException exception)
        {
            logger.LogError(exception, "Failed to add user {User}", user);
            throw;
        }
    }

    private static User MapToUser(UserEntity entity) => new()
    {
        IdentityProvider = entity.PartitionKey,
        Id = entity.RowKey,
        Name = entity.Name,
        Email = entity.Email,
        Role = entity.Role
    };

    private static UserEntity MapToEntity(User user) => new()
    {
        PartitionKey = user.IdentityProvider,
        RowKey = user.Id,
        Name = user.Name,
        Email = user.Email,
        Role = user.Role
    };
}
