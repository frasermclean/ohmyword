using FluentResults;
using OhMyWord.Core.Models;
using OhMyWord.Integrations.Models.Entities;
using OhMyWord.Integrations.Models.WordsApi;
using OhMyWord.Integrations.Services.RapidApi.WordsApi;
using OhMyWord.Integrations.Services.Repositories;

namespace OhMyWord.Domain.Services;

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

    /// <summary>
    /// Generates definitions for a word from a dictionary lookup.
    /// </summary>
    /// <param name="wordId">The word to search for</param>
    /// <param name="partOfSpeech">Optional part of speech to restrict results to</param>
    /// <param name="cancellationToken">Task cancellation token</param>
    /// <returns>A collection of definitions that match the parameters</returns>
    Task<IEnumerable<Definition>> GenerateDefinitionsAsync(string wordId, PartOfSpeech? partOfSpeech = default,
        CancellationToken cancellationToken = default);
}

public class DefinitionsService : IDefinitionsService
{
    private readonly IDefinitionsRepository definitionsRepository;
    private readonly IWordsApiClient wordsApiClient;

    public DefinitionsService(IDefinitionsRepository definitionsRepository, IWordsApiClient wordsApiClient)
    {
        this.definitionsRepository = definitionsRepository;
        this.wordsApiClient = wordsApiClient;
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

    public async Task<IEnumerable<Definition>> GenerateDefinitionsAsync(string wordId,
        PartOfSpeech? partOfSpeech = default, CancellationToken cancellationToken = default)
    {
        var details = await wordsApiClient.GetWordDetailsAsync(wordId, cancellationToken);

        return details is null
            ? Enumerable.Empty<Definition>()
            : details.DefinitionResults.Select(MapToDefinition)
                .Where(definition => definition.PartOfSpeech == partOfSpeech || partOfSpeech is null);
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

    private static Definition MapToDefinition(DefinitionResult result) => new()
    {
        Value = result.Definition,
        PartOfSpeech = Enum.Parse<PartOfSpeech>(result.PartOfSpeech),
        Example = result.Examples.FirstOrDefault()
    };
}
