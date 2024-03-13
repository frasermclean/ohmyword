using Azure.Data.Tables;
using FluentResults;
using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using OhMyWord.Integrations.Storage.Models;

namespace OhMyWord.Integrations.Storage.Services;

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
        return filter is null
            ? tableClient.QueryAsync<UserEntity>(cancellationToken: cancellationToken).Select(MapToUser)
            : tableClient.QueryAsync<UserEntity>(entity =>
                    entity.PartitionKey.Contains(filter, StringComparison.InvariantCultureIgnoreCase) ||
                    entity.RowKey.Contains(filter, StringComparison.InvariantCultureIgnoreCase) ||
                    entity.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase) ||
                    entity.Email.Contains(filter, StringComparison.InvariantCultureIgnoreCase),
                cancellationToken: cancellationToken).Select(MapToUser);
    }

    public async Task<Result<User>> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var entity = await tableClient
            .QueryAsync<UserEntity>(entity => entity.RowKey == userId, cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
            logger.LogWarning("User with ID: {UserId} was not found", userId);

        return entity is null
            ? Result.Fail($"User with ID: {userId} was not found")
            : MapToUser(entity);
    }

    public async Task<Result> CreateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(user);
        var response = await tableClient.UpsertEntityAsync(entity, cancellationToken: cancellationToken);
        return response.IsError
            ? Result.Fail(response.ReasonPhrase)
            : Result.Ok();
    }

    private static UserEntity MapToEntity(User user) => new()
    {
        PartitionKey = user.IdentityProvider,
        RowKey = user.Id,
        Name = user.Name,
        Email = user.Email,
        Role = user.Role
    };

    private static User MapToUser(UserEntity entity) => new()
    {
        Id = entity.RowKey,
        Name = entity.Name,
        Email = entity.Email,
        IdentityProvider = entity.PartitionKey,
        Role = entity.Role
    };
}
