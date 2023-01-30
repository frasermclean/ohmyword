using Microsoft.Extensions.Logging;
using OhMyWord.Data.Entities;

namespace OhMyWord.Data.Services;

public interface IDefinitionsRepository
{
    Task<DefinitionEntity> CreateDefinitionAsync(DefinitionEntity definitionEntity);
}

public class DefinitionsRepository : Repository<DefinitionEntity>, IDefinitionsRepository
{
    public DefinitionsRepository(IDatabaseService databaseService, ILogger<DefinitionsRepository> logger)
        : base(databaseService, logger, "definitions")
    {
    }

    public Task<DefinitionEntity> CreateDefinitionAsync(DefinitionEntity definitionEntity) =>
        CreateItemAsync(definitionEntity);
}
