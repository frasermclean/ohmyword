using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Infrastructure.Entities;
using OhMyWord.Infrastructure.Options;

namespace OhMyWord.Infrastructure.Services;

public interface IUsersRepository
{
    Task<UserEntity?> GetUserAsync(string userId, CancellationToken cancellationToken = default);
    Task CreateUserAsync(UserEntity userEntity, CancellationToken cancellationToken = default);
    Task UpdateUserAsync(UserEntity userEntity, CancellationToken cancellationToken = default);
}

public class UsersRepository : Repository<UserEntity>, IUsersRepository
{
    public UsersRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options,
        ILogger<UsersRepository> logger)
        : base(cosmosClient, options, logger, "users")
    {
    }

    public Task<UserEntity?> GetUserAsync(string userId, CancellationToken cancellationToken) =>
        ReadItemAsync(userId, userId, cancellationToken);

    public Task CreateUserAsync(UserEntity userEntity, CancellationToken cancellationToken) =>
        CreateItemAsync(userEntity, cancellationToken);

    public Task UpdateUserAsync(UserEntity userEntity, CancellationToken cancellationToken = default)
        => UpdateItemAsync(userEntity, cancellationToken);
}
