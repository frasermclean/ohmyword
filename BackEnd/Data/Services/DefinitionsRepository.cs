﻿using Microsoft.Extensions.Logging;
using OhMyWord.Data.Entities;

namespace OhMyWord.Data.Services;

public interface IDefinitionsRepository
{
    Task CreateDefinitionAsync(DefinitionEntity definitionEntity);

    IAsyncEnumerable<DefinitionEntity> GetDefinitionsAsync(string wordId,
        CancellationToken cancellationToken = default);
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
        ReadPartitionItemsAsync(wordId, cancellationToken);
}
