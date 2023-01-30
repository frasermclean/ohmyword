using Microsoft.Extensions.Logging;
using OhMyWord.Data.Entities;

namespace OhMyWord.Data.Services;

public interface IDefinitionsRepository
{
    Task CreateDefinitionAsync(DefinitionEntity definitionEntity);

    IAsyncEnumerable<DefinitionEntity> GetDefinitionsAsync(string wordId,
        CancellationToken cancellationToken = default);

    Task DeleteDefinitionsAsync(string wordId, CancellationToken cancellationToken);
}

public class DefinitionsRepository : Repository<DefinitionEntity>, IDefinitionsRepository
{
    public DefinitionsRepository(IDatabaseService databaseService, ILogger<DefinitionsRepository> logger)
        : base(databaseService, logger, "definitions")
    {
    }

    public Task CreateDefinitionAsync(DefinitionEntity definitionEntity) =>
        CreateItemAsync(definitionEntity);

    public IAsyncEnumerable<DefinitionEntity> GetDefinitionsAsync(string wordId,
        CancellationToken cancellationToken = default) =>
        ReadPartitionItems(wordId, cancellationToken);

    public async Task DeleteDefinitionsAsync(string wordId, CancellationToken cancellationToken)
    {
        var itemIds = await ReadItemIds(wordId, cancellationToken).ToListAsync(cancellationToken);
        await Task.WhenAll(itemIds.Select(id => DeleteItemAsync(id, wordId, cancellationToken)));
    }
}
