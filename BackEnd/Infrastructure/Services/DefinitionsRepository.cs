using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Infrastructure.Entities;
using OhMyWord.Infrastructure.Options;

namespace OhMyWord.Infrastructure.Services;

public interface IDefinitionsRepository
{
    IAsyncEnumerable<DefinitionEntity> GetDefinitionsAsync(string wordId,
        CancellationToken cancellationToken = default);

    Task CreateDefinitionAsync(DefinitionEntity definitionEntity, CancellationToken cancellationToken = default);
    Task UpdateDefinitionAsync(DefinitionEntity definitionEntity, CancellationToken cancellationToken = default);
    Task DeleteDefinitionsAsync(string wordId, CancellationToken cancellationToken = default);
}

public class DefinitionsRepository : Repository<DefinitionEntity>, IDefinitionsRepository
{
    public DefinitionsRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options,
        ILogger<DefinitionsRepository> logger)
        : base(cosmosClient, options, logger, "definitions")
    {
    }

    public IAsyncEnumerable<DefinitionEntity> GetDefinitionsAsync(string wordId, CancellationToken cancellationToken) =>
        ReadPartitionItems(wordId, cancellationToken);

    public Task CreateDefinitionAsync(DefinitionEntity definitionEntity, CancellationToken cancellationToken) =>
        CreateItemAsync(definitionEntity, cancellationToken);

    public Task UpdateDefinitionAsync(DefinitionEntity definitionEntity, CancellationToken cancellationToken)
        => UpdateItemAsync(definitionEntity, cancellationToken);

    public async Task DeleteDefinitionsAsync(string wordId, CancellationToken cancellationToken)
    {
        var itemIds = await ReadItemIds(wordId, cancellationToken).ToListAsync(cancellationToken);
        await Task.WhenAll(itemIds.Select(id => DeleteItemAsync(id, wordId, cancellationToken)));
    }
}
