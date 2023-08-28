using FluentResults;
using OhMyWord.Core.Models;

namespace OhMyWord.Logic.Services;

public interface IDefinitionsService
{
    IAsyncEnumerable<Definition> GetDefinitions(string wordId, CancellationToken cancellationToken = default);

    Task<Result<Definition>> GetDefinitionAsync(string wordId, string definitionId,
        CancellationToken cancellationToken = default);

    Task<Result<Definition>> CreateDefinitionAsync(string wordId, Definition definition,
        CancellationToken cancellationToken = default);

    Task<Result<Definition>> UpdateDefinitionAsync(string wordId, Definition definition,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteDefinitionsAsync(string wordId, CancellationToken cancellationToken = default);
}

public class DefinitionsService : IDefinitionsService
{
    private readonly IDefinitionsRepository definitionsRepository;

    public DefinitionsService(IDefinitionsRepository definitionsRepository)
    {
        this.definitionsRepository = definitionsRepository;
    }

    public IAsyncEnumerable<Definition> GetDefinitions(string wordId, CancellationToken cancellationToken)
        => definitionsRepository.GetDefinitions(wordId, cancellationToken)
            .Select(MapToDefinition);

    public async Task<Result<Definition>> GetDefinitionAsync(string wordId, string definitionId,
        CancellationToken cancellationToken)
    {
        var result = await definitionsRepository.GetDefinitionAsync(wordId, definitionId, cancellationToken);
        return result.Map(MapToDefinition);
    }

    public async Task<Result<Definition>> CreateDefinitionAsync(string wordId, Definition definition,
        CancellationToken cancellationToken = default)
    {
        var result =
            await definitionsRepository.CreateDefinitionAsync(MapToEntity(wordId, definition), cancellationToken);

        return result.Map(MapToDefinition);
    }

    public async Task<Result<Definition>> UpdateDefinitionAsync(string wordId, Definition definition,
        CancellationToken cancellationToken = default)
    {
        var result =
            await definitionsRepository.UpdateDefinitionAsync(MapToEntity(wordId, definition), cancellationToken);

        return result.Map(MapToDefinition);
    }

    public async Task<Result> DeleteDefinitionsAsync(string wordId, CancellationToken cancellationToken = default)
    {
        var definitionIds = await definitionsRepository.GetDefinitionIds(wordId, cancellationToken)
            .ToListAsync(cancellationToken);

        var results = await Task.WhenAll(definitionIds.Select(definitionId =>
            definitionsRepository.DeleteDefinitionAsync(wordId, definitionId, cancellationToken)));

        return results.Merge();
    }

    private static DefinitionEntity MapToEntity(string wordId, Definition definition) => new()
    {
        Id = definition.Id.ToString(),
        PartOfSpeech = definition.PartOfSpeech,
        Value = definition.Value,
        Example = definition.Example,
        WordId = wordId,
    };

    private static Definition MapToDefinition(DefinitionEntity entity) => new()
    {
        Id = Guid.Parse(entity.Id), PartOfSpeech = entity.PartOfSpeech, Value = entity.Value, Example = entity.Example,
    };
}
