using FluentResults;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Integrations.CosmosDb.Models.Entities;
using OhMyWord.Integrations.CosmosDb.Options;

namespace OhMyWord.Integrations.CosmosDb.Services;

public interface ISessionsRepository
{
    Task<Result<SessionEntity>> CreateSessionAsync(SessionEntity entity, CancellationToken cancellationToken = default);
}

public class SessionsRepository : Repository<SessionEntity>, ISessionsRepository
{
    public SessionsRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options,
        ILogger<SessionsRepository> logger)
        : base(cosmosClient, logger, options.Value.DatabaseId, "sessions")
    {
    }

    public Task<Result<SessionEntity>> CreateSessionAsync(SessionEntity entity,
        CancellationToken cancellationToken = default)
        => CreateItemAsync(entity, cancellationToken);
}
