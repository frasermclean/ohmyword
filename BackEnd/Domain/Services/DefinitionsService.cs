using FluentResults;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Models.WordsApi;
using OhMyWord.Infrastructure.Services;
using OhMyWord.Infrastructure.Services.RapidApi.WordsApi;

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
                .Where(definition => definition.PartOfSpeech != PartOfSpeech.Unknown &&
                                     (partOfSpeech is null || definition.PartOfSpeech == partOfSpeech));
    }

    private static DefinitionEntity MapToEntity(string wordId, Definition definition) => new()
    {
        Id = definition.Id.ToString(),
        PartOfSpeech = definition.PartOfSpeech,
        Value = definition.Value,
        Example = definition.Example,
        WordId = wordId,
    };

    private static Definition MapToDefinition(DefinitionEntity definitionEntity) => new()
    {
        Id = Guid.Parse(definitionEntity.Id),
        PartOfSpeech = definitionEntity.PartOfSpeech,
        Value = definitionEntity.Value,
        Example = definitionEntity.Example,
    };

    private static Definition MapToDefinition(DefinitionResult definitionResult) => new()
    {
        Value = definitionResult.Definition,
        PartOfSpeech = Enum.TryParse<PartOfSpeech>(definitionResult.PartOfSpeech, true, out var partOfSpeech)
            ? partOfSpeech
            : PartOfSpeech.Unknown,
        Example = definitionResult.Examples.FirstOrDefault()
    };
}
