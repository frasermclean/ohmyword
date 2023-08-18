using FluentResults;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Integrations.Models.Entities;
using OhMyWord.Integrations.Options;

namespace OhMyWord.Integrations.Services.Repositories;

public interface IRoundsRepository
{
    Task<Result<RoundEntity>> CreateRoundAsync(RoundEntity round, CancellationToken cancellationToken = default);
}

public class RoundsRepository : Repository<RoundEntity>, IRoundsRepository
{
    public RoundsRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options,
        ILogger<RoundsRepository> logger)
        : base(cosmosClient, logger, options.Value.DatabaseId, "rounds")
    {
    }

    public Task<Result<RoundEntity>> CreateRoundAsync(RoundEntity entity, CancellationToken cancellationToken = default)
        => CreateItemAsync(entity, cancellationToken);
}
