using FluentResults;
using OhMyWord.Core.Models;
using OhMyWord.Core.Services;

namespace OhMyWord.Logic.Services;

public interface IWordsService
{
    IAsyncEnumerable<Word> SearchWords(int offset = IWordsRepository.OffsetMinimum,
        int limit = IWordsRepository.LimitDefault, string filter = "", string orderBy = "", bool isDescending = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Read all word IDs from the database.
    /// </summary>
    /// <param name="cancellationToken">Task cancellation token</param>
    /// <returns>All unique word IDs</returns>
    IAsyncEnumerable<string> GetAllWordIds(CancellationToken cancellationToken);

    /// <summary>
    /// Get the total number of words in the database.
    /// </summary>
    /// <param name="cancellationToken">Task cancellation token</param>
    /// <returns>The total word count.</returns>
    Task<int> GetTotalWordCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a word by its ID.
    /// </summary>
    /// <param name="wordId">The word to attempt to find.</param>
    /// <param name="performExternalLookup">If set to true, will search for the word using an external service.</param>
    /// <param name="cancellationToken">Task cancellation token.</param>
    /// <returns>Success if found, failure if not.</returns>
    Task<Result<Word>> GetWordAsync(string wordId, bool performExternalLookup = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new word.
    /// </summary>
    /// <param name="word">The <see cref="Word"/> to create.</param>
    /// <param name="cancellationToken">Task cancellation token.</param>
    /// <returns>Success if created, failure if not.</returns>
    Task<Result<Word>> CreateWordAsync(Word word, CancellationToken cancellationToken = default);

    Task<Result<Word>> UpdateWordAsync(Word word, CancellationToken cancellationToken = default);
    Task<Result> DeleteWordAsync(string wordId, CancellationToken cancellationToken = default);
}

public class WordsService : IWordsService
{
    private readonly IWordsRepository wordsRepository;
    private readonly IDefinitionsService definitionsService;
    private readonly IDictionaryClient dictionaryClient;

    public WordsService(IWordsRepository wordsRepository, IDefinitionsService definitionsService,
        IDictionaryClient dictionaryClient)
    {
        this.wordsRepository = wordsRepository;
        this.definitionsService = definitionsService;
        this.dictionaryClient = dictionaryClient;
    }

    public IAsyncEnumerable<Word> SearchWords(int offset, int limit, string filter, string orderBy, bool isDescending,
        CancellationToken cancellationToken = default) =>
        wordsRepository.SearchWords(offset, limit, filter, orderBy, isDescending, cancellationToken)
            .SelectAwait(async wordEntity =>
            {
                var definitions = await definitionsService.GetDefinitions(wordEntity.Id, cancellationToken)
                    .ToListAsync(cancellationToken);
                return MapToWord(wordEntity, definitions);
            });

    public IAsyncEnumerable<string> GetAllWordIds(CancellationToken cancellationToken) =>
        wordsRepository.GetAllWordIds(cancellationToken);

    public Task<int> GetTotalWordCountAsync(CancellationToken cancellationToken = default) =>
        wordsRepository.GetTotalWordCountAsync(cancellationToken);

    public async Task<Result<Word>> GetWordAsync(string wordId, bool performExternalLookup,
        CancellationToken cancellationToken = default)
    {
        // lookup up using external service if requested
        if (performExternalLookup)
        {
            var word = await dictionaryClient.GetWordAsync(wordId, cancellationToken);
            if (word is not null)
                return word;
        }

        var wordResult = await wordsRepository.GetWordAsync(wordId, cancellationToken);

        return wordResult.IsSuccess
            ? MapToWord(wordResult.Value, await definitionsService
                .GetDefinitions(wordId, cancellationToken)
                .ToListAsync(cancellationToken))
            : wordResult.ToResult();
    }

    public async Task<Result<Word>> CreateWordAsync(Word word, CancellationToken cancellationToken = default)
    {
        // create word entity
        var wordResult = await wordsRepository.CreateWordAsync(MapToEntity(word), cancellationToken);
        if (wordResult.IsFailed)
            return wordResult.ToResult();

        // create definition entities
        var definitionResults = await Task.WhenAll(word.Definitions.Select(definition =>
            definitionsService.CreateDefinitionAsync(word.Id, definition, cancellationToken)));
        if (definitionResults.Any(result => result.IsFailed))
            return definitionResults.Merge().ToResult();

        return MapToWord(wordResult.Value, definitionResults.Select(result => result.Value));
    }

    public async Task<Result<Word>> UpdateWordAsync(Word word, CancellationToken cancellationToken = default)
    {
        var wordResult = await wordsRepository.UpdateWordAsync(MapToEntity(word), cancellationToken);

        if (wordResult.IsFailed)
            return wordResult.ToResult();

        var definitionResults = await Task.WhenAll(word.Definitions.Select(definition =>
            definitionsService.UpdateDefinitionAsync(word.Id, definition, cancellationToken)));

        return definitionResults.All(result => result.IsSuccess)
            ? MapToWord(wordResult.Value, definitionResults.Select(result => result.Value))
            : definitionResults.Merge().ToResult();
    }

    public async Task<Result> DeleteWordAsync(string wordId, CancellationToken cancellationToken = default)
    {
        var deleteWordResult = await wordsRepository.DeleteWordAsync(wordId, cancellationToken);
        if (deleteWordResult.IsFailed)
            return deleteWordResult;

        var deleteDefinitionsResult = await definitionsService.DeleteDefinitionsAsync(wordId, cancellationToken);
        return deleteDefinitionsResult;
    }
}
