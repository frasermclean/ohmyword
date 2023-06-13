using FluentResults;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Options;

namespace OhMyWord.Infrastructure.Services;

public interface IRoundsRepository
{
    Task<Result<RoundEntity>> CreateRoundAsync(RoundEntity round, CancellationToken cancellationToken = default);
}

public class RoundsRepository : Repository<RoundEntity>, IRoundsRepository
{
    public RoundsRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options,
        ILogger<RoundsRepository> logger)
        : base(cosmosClient, options, logger, "rounds")
    {
    }

    public Task<Result<RoundEntity>> CreateRoundAsync(RoundEntity entity, CancellationToken cancellationToken = default)
        => CreateItemAsync(entity, cancellationToken);
}
