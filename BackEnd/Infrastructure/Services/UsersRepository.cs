using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using OhMyWord.Infrastructure.Entities;

namespace OhMyWord.Infrastructure.Services;

public interface IUsersRepository
{
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

    public async Task<UserEntity?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await tableClient
            .QueryAsync<UserEntity>(entity => entity.PartitionKey == userId, cancellationToken: cancellationToken)
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
