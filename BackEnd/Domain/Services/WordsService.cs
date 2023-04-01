using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Results;
using OhMyWord.Infrastructure.Entities;
using OhMyWord.Infrastructure.Services;
using OneOf;
using OneOf.Types;
using System.Net;

namespace OhMyWord.Domain.Services;

public interface IWordsService
{
    IAsyncEnumerable<Word> SearchWords(int offset = WordsRepository.OffsetMinimum,
        int limit = WordsRepository.LimitDefault, string filter = "",
        SearchWordsOrderBy orderBy = SearchWordsOrderBy.Id,
        SortDirection direction = SortDirection.Ascending, CancellationToken cancellationToken = default);

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

    Task<OneOf<Word, NotFound>> GetWordAsync(string wordId, CancellationToken cancellationToken = default);
    Task<OneOf<Word, NotFound, Conflict>> CreateWordAsync(Word word, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new word with the given ID automatically populated with definitions from the dictionary.
    /// </summary>
    /// <param name="wordId">The word to create</param>
    /// <param name="cancellationToken">Task cancellation token</param>
    /// <returns></returns>
    Task<OneOf<Word, NotFound, Conflict>> CreateWordAsync(string wordId, CancellationToken cancellationToken = default);

    Task UpdateWordAsync(Word word, CancellationToken cancellationToken = default);
    Task DeleteWordAsync(string wordId, CancellationToken cancellationToken = default);
}

public class WordsService : IWordsService
{
    private readonly ILogger<WordsService> logger;
    private readonly IWordsRepository wordsRepository;
    private readonly IDefinitionsRepository definitionsRepository;
    private readonly IDictionaryService dictionaryService;

    public WordsService(ILogger<WordsService> logger, IWordsRepository wordsRepository,
        IDefinitionsRepository definitionsRepository, IDictionaryService dictionaryService)
    {
        this.logger = logger;
        this.wordsRepository = wordsRepository;
        this.definitionsRepository = definitionsRepository;
        this.dictionaryService = dictionaryService;
    }

    public IAsyncEnumerable<Word> SearchWords(int offset, int limit, string filter, SearchWordsOrderBy orderBy,
        SortDirection direction, CancellationToken cancellationToken = default) =>
        wordsRepository.SearchWords(offset, limit, filter, orderBy, direction, cancellationToken)
            .SelectAwait(async wordEntity => await MapToWordAsync(wordEntity, cancellationToken));

    public IAsyncEnumerable<string> GetAllWordIds(CancellationToken cancellationToken) =>
        wordsRepository.GetAllWordIds(cancellationToken);

    public Task<int> GetTotalWordCountAsync(CancellationToken cancellationToken = default) =>
        wordsRepository.GetTotalWordCountAsync(cancellationToken);

    public async Task<OneOf<Word, NotFound>> GetWordAsync(string wordId, CancellationToken cancellationToken = default)
    {
        var wordEntity = await wordsRepository.GetWordAsync(wordId, cancellationToken);
        return wordEntity is not null
            ? await MapToWordAsync(wordEntity, cancellationToken)
            : new NotFound();
    }

    public async Task<OneOf<Word, NotFound, Conflict>> CreateWordAsync(string wordId,
        CancellationToken cancellationToken = default)
    {
        var dictionaryWords = (await dictionaryService.LookupWordAsync(wordId, cancellationToken)).ToList();

        if (dictionaryWords.Count == 0) return new NotFound();

        var words = dictionaryWords
            .Take(1)
            .Where(dictionaryWord =>
                string.Equals(dictionaryWord.Metadata.Stems.First(), wordId, StringComparison.CurrentCultureIgnoreCase))
            .Select(dictionaryWord => dictionaryWord.ToWord());

        return await CreateWordAsync(words.First(), cancellationToken);
    }

    public async Task<OneOf<Word, NotFound, Conflict>> CreateWordAsync(Word word,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await wordsRepository.CreateWordAsync(word.ToEntity(), cancellationToken);
        }
        catch (CosmosException cosmosException) when (cosmosException.StatusCode == HttpStatusCode.Conflict)
        {
            logger.LogWarning(cosmosException, "Word with ID: {WordId} already exists", word.Id);
            return new Conflict($"Word with ID: {word.Id} already exists");
        }

        await Task.WhenAll(word.Definitions.Select(definition =>
            definitionsRepository.CreateDefinitionAsync(definition.ToEntity(word.Id), cancellationToken)));

        return word;
    }

    public async Task UpdateWordAsync(Word word, CancellationToken cancellationToken = default)
    {
        await wordsRepository.UpdateWordAsync(
            new WordEntity { Id = word.Id, DefinitionCount = word.Definitions.Count() },
            cancellationToken);

        await Task.WhenAll(word.Definitions.Select(definition =>
            definitionsRepository.UpdateDefinitionAsync(definition.ToEntity(word.Id), cancellationToken)));
    }

    public async Task DeleteWordAsync(string wordId, CancellationToken cancellationToken = default)
    {
        await wordsRepository.DeleteWordAsync(wordId, cancellationToken);
        await definitionsRepository.DeleteDefinitionsAsync(wordId, cancellationToken);
    }

    private async Task<Word> MapToWordAsync(Entity wordEntity, CancellationToken cancellationToken) => new()
    {
        Id = wordEntity.Id,
        Definitions = await definitionsRepository.GetDefinitionsAsync(wordEntity.Id, cancellationToken)
            .Select(Definition.FromEntity)
            .ToListAsync(cancellationToken),
        LastModifiedTime = wordEntity.LastModifiedTime,
    };
}
