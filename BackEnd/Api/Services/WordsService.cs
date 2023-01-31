using OhMyWord.Core.Extensions;
using OhMyWord.Core.Models;
using OhMyWord.Data.Entities;
using OhMyWord.Data.Services;

namespace OhMyWord.Api.Services;

public interface IWordsService
{
    IAsyncEnumerable<Word> ListWordsAsync(int offset = WordsRepository.OffsetMinimum,
        int limit = WordsRepository.LimitDefault, string filter = "", ListWordsOrderBy orderBy = ListWordsOrderBy.Id,
        SortDirection direction = SortDirection.Ascending, CancellationToken cancellationToken = default);


    /// <summary>
    /// Read all word IDs from the database.
    /// </summary>
    /// <param name="cancellationToken">Task cancellation token</param>
    /// <returns>All unique word IDs</returns>
    IAsyncEnumerable<string> GetAllWordIds(CancellationToken cancellationToken);

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

    public IAsyncEnumerable<Word> ListWordsAsync(int offset = WordsRepository.OffsetMinimum,
        int limit = WordsRepository.LimitDefault, string filter = "", ListWordsOrderBy orderBy = ListWordsOrderBy.Id,
        SortDirection direction = SortDirection.Ascending, CancellationToken cancellationToken = default) =>
        wordsRepository.ListWords(offset, limit, filter, orderBy, direction, cancellationToken)
            .SelectAwait(async wordEntity => await MapToWordAsync(wordEntity, cancellationToken));

    public IAsyncEnumerable<string> GetAllWordIds(CancellationToken cancellationToken) =>
        wordsRepository.GetAllWordIds(cancellationToken);

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
        LastModified = wordEntity.LastModifiedTime,
    };
}
