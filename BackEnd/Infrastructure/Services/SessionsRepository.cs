using FluentResults;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Options;

namespace OhMyWord.Infrastructure.Services;

public interface ISessionsRepository
{
    Task<Result<SessionEntity>> CreateSessionAsync(SessionEntity entity, CancellationToken cancellationToken = default);
}

public class SessionsRepository : Repository<SessionEntity>, ISessionsRepository
{
    public SessionsRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options,
        ILogger<SessionsRepository> logger)
        : base(cosmosClient, options, logger, "sessions")
    {
    }

    public Task<Result<SessionEntity>> CreateSessionAsync(SessionEntity entity, CancellationToken cancellationToken = default)
        => CreateItemAsync(entity, cancellationToken);
}
