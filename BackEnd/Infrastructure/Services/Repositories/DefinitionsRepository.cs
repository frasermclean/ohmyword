using FluentResults;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Options;

namespace OhMyWord.Infrastructure.Services.Repositories;

public interface IDefinitionsRepository
{
    IAsyncEnumerable<DefinitionEntity> GetDefinitions(string wordId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> GetDefinitionIds(string wordId, CancellationToken cancellationToken = default);

    Task<Result<DefinitionEntity>> GetDefinitionAsync(string wordId, string definitionId,
        CancellationToken cancellationToken = default);

    Task<Result<DefinitionEntity>> CreateDefinitionAsync(DefinitionEntity definitionEntity,
        CancellationToken cancellationToken = default);

    Task<Result<DefinitionEntity>> UpdateDefinitionAsync(DefinitionEntity definitionEntity, CancellationToken cancellationToken = default);

    Task<Result> DeleteDefinitionAsync(string wordId, string definitionId,
        CancellationToken cancellationToken = default);
}

public class DefinitionsRepository : Repository<DefinitionEntity>, IDefinitionsRepository
{
    public DefinitionsRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options,
        ILogger<DefinitionsRepository> logger)
        : base(cosmosClient, options, logger, "definitions")
    {
    }

    public IAsyncEnumerable<DefinitionEntity> GetDefinitions(string wordId, CancellationToken cancellationToken)
        => ReadPartitionItems(wordId, cancellationToken);

    public IAsyncEnumerable<string> GetDefinitionIds(string wordId, CancellationToken cancellationToken = default)
        => ReadItemIds(wordId, cancellationToken);

    public Task<Result<DefinitionEntity>> GetDefinitionAsync(string wordId, string definitionId,
        CancellationToken cancellationToken = default)
        => ReadItemAsync(definitionId, wordId, cancellationToken);

    public Task<Result<DefinitionEntity>> CreateDefinitionAsync(DefinitionEntity definitionEntity,
        CancellationToken cancellationToken)
        => CreateItemAsync(definitionEntity, cancellationToken);

    public Task<Result<DefinitionEntity>> UpdateDefinitionAsync(DefinitionEntity definitionEntity, CancellationToken cancellationToken)
        => ReplaceItemAsync(definitionEntity, cancellationToken);

    public Task<Result> DeleteDefinitionAsync(string wordId, string definitionId,
        CancellationToken cancellationToken = default)
        => DeleteItemAsync(definitionId, wordId, cancellationToken);
}
