using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Enums;
using OhMyWord.WordsApi.Services;

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
    private readonly IWordsApiClient wordsApiClient;

    public DefinitionsService(IWordsApiClient wordsApiClient)
    {
        this.wordsApiClient = wordsApiClient;
    }

    public async Task<IEnumerable<Definition>> GenerateDefinitionsAsync(string wordId,
        PartOfSpeech? partOfSpeech = default, CancellationToken cancellationToken = default)
    {
        var details = await wordsApiClient.GetWordDetailsAsync(wordId, cancellationToken);

        return details is not null
            ? details.DefinitionResults
                .Select(result => result.ToDefinition())
                .Where(definition => definition.PartOfSpeech != PartOfSpeech.Unknown &&
                                     (partOfSpeech is null || definition.PartOfSpeech == partOfSpeech))
            : Enumerable.Empty<Definition>();
    }
}
