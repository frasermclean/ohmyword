using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Enums;
using OhMyWord.Infrastructure.Services;

namespace OhMyWord.Domain.Services;

public interface IDefinitionsService
{
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
    private readonly IDictionaryService dictionaryService;

    public DefinitionsService(IDictionaryService dictionaryService)
    {
        this.dictionaryService = dictionaryService;
    }

    public async Task<IEnumerable<Definition>> GenerateDefinitionsAsync(string wordId,
        PartOfSpeech? partOfSpeech = default, CancellationToken cancellationToken = default)
        => (await dictionaryService.LookupWordAsync(wordId, cancellationToken))
            .Select(dictionaryWord => dictionaryWord.ToWord())
            .Where(word => word.Id == wordId)
            .SelectMany(word => word.Definitions)
            .Where(definition => FilterDefinition(definition, partOfSpeech));

    private static bool FilterDefinition(Definition definition, PartOfSpeech? partOfSpeech)
        => definition.PartOfSpeech != PartOfSpeech.Unknown &&
           (partOfSpeech is null || definition.PartOfSpeech == partOfSpeech);
}
