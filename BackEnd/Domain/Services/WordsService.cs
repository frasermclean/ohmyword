using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Entities;
using OhMyWord.Infrastructure.Services;

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

    Task<Word?> GetWordAsync(string wordId, CancellationToken cancellationToken = default);
    Task CreateWordAsync(Word word, CancellationToken cancellationToken = default);
    Task UpdateWordAsync(Word word, CancellationToken cancellationToken = default);
    Task DeleteWordAsync(string wordId, CancellationToken cancellationToken = default);
}

public class WordsService : IWordsService
{
    private readonly IWordsRepository wordsRepository;
    private readonly IDefinitionsRepository definitionsRepository;

    public WordsService(IWordsRepository wordsRepository, IDefinitionsRepository definitionsRepository)
    {
        this.wordsRepository = wordsRepository;
        this.definitionsRepository = definitionsRepository;
    }

    public IAsyncEnumerable<Word> SearchWords(int offset, int limit, string filter, SearchWordsOrderBy orderBy,
        SortDirection direction, CancellationToken cancellationToken = default) =>
        wordsRepository.SearchWords(offset, limit, filter, orderBy, direction, cancellationToken)
            .SelectAwait(async wordEntity => await MapToWordAsync(wordEntity, cancellationToken));

    public IAsyncEnumerable<string> GetAllWordIds(CancellationToken cancellationToken) =>
        wordsRepository.GetAllWordIds(cancellationToken);

    public Task<int> GetTotalWordCountAsync(CancellationToken cancellationToken = default) =>
        wordsRepository.GetTotalWordCountAsync(cancellationToken);

    public async Task<Word?> GetWordAsync(string wordId, CancellationToken cancellationToken = default)
    {
        var wordEntity = await wordsRepository.GetWordAsync(wordId, cancellationToken);
        return wordEntity is null ? default : await MapToWordAsync(wordEntity, cancellationToken);
    }

    public async Task CreateWordAsync(Word word, CancellationToken cancellationToken = default)
    {
        await wordsRepository.CreateWordAsync(
            new WordEntity { Id = word.Id, DefinitionCount = word.Definitions.Count() },
            cancellationToken);

        await Task.WhenAll(word.Definitions.Select(definition =>
            definitionsRepository.CreateDefinitionAsync(definition.ToEntity(word.Id), cancellationToken)));
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
