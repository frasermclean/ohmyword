using Microsoft.Extensions.Logging;
using OhMyWord.Data.Entities;

namespace OhMyWord.Data.Services;

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
    public DefinitionsRepository(IDatabaseService databaseService, ILogger<DefinitionsRepository> logger)
        : base(databaseService, logger, "definitions")
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
