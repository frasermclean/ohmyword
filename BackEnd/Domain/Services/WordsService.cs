using FluentResults;
using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Services.Repositories;

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

    Task<Result<Word>> GetWordAsync(string wordId, CancellationToken cancellationToken = default);
    Task<Result<Word>> CreateWordAsync(Word word, CancellationToken cancellationToken = default);

    Task<Result<Word>> UpdateWordAsync(Word word, CancellationToken cancellationToken = default);
    Task<Result> DeleteWordAsync(string wordId, CancellationToken cancellationToken = default);
}

public class WordsService : IWordsService
{
    private readonly ILogger<WordsService> logger;
    private readonly IWordsRepository wordsRepository;
    private readonly IDefinitionsService definitionsService;

    public WordsService(ILogger<WordsService> logger, IWordsRepository wordsRepository,
        IDefinitionsService definitionsService)
    {
        this.logger = logger;
        this.wordsRepository = wordsRepository;
        this.definitionsService = definitionsService;
    }

    public IAsyncEnumerable<Word> SearchWords(int offset, int limit, string filter, SearchWordsOrderBy orderBy,
        SortDirection direction, CancellationToken cancellationToken = default) =>
        wordsRepository.SearchWords(offset, limit, filter, orderBy, direction, cancellationToken)
            .SelectAwait(async wordEntity => await MapToWordAsync(wordEntity, cancellationToken));

    public IAsyncEnumerable<string> GetAllWordIds(CancellationToken cancellationToken) =>
        wordsRepository.GetAllWordIds(cancellationToken);

    public Task<int> GetTotalWordCountAsync(CancellationToken cancellationToken = default) =>
        wordsRepository.GetTotalWordCountAsync(cancellationToken);

    public async Task<Result<Word>> GetWordAsync(string wordId, CancellationToken cancellationToken = default)
    {
        var result = await wordsRepository.GetWordAsync(wordId, cancellationToken);

        return result.IsSuccess
            ? await MapToWordAsync(result.Value, cancellationToken)
            : result.ToResult();
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

        definitionResults.Merge();
        if (definitionResults.Any(result => result.IsFailed))
        {
        }


        return MapToWord(wordResult.Value, definitionResults.Select(result => result.Value));
    }

    public async Task<Result> DeleteWordAsync(string wordId, CancellationToken cancellationToken = default)
    {
        var deleteWordResult = await wordsRepository.DeleteWordAsync(wordId, cancellationToken);
        if (deleteWordResult.IsFailed)
            return deleteWordResult;

        var deleteDefinitionsResult = await definitionsService.DeleteDefinitionsAsync(wordId, cancellationToken);
        return deleteDefinitionsResult;
    }

    private static WordEntity MapToEntity(Word word) => new()
    {
        Id = word.Id, DefinitionCount = word.Definitions.Count(),
    };

    private static Word MapToWord(Entity entity, IEnumerable<Definition> definitions) => new()
    {
        Id = entity.Id, Definitions = definitions, LastModifiedTime = entity.LastModifiedTime
    };

    private async Task<Word> MapToWordAsync(Entity entity, CancellationToken cancellationToken) => new()
    {
        Id = entity.Id,
        Definitions = await definitionsService.GetDefinitions(entity.Id, cancellationToken)
            .ToListAsync(cancellationToken),
        LastModifiedTime = entity.LastModifiedTime,
    };
}
