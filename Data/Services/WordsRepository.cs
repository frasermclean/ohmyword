using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using OhMyWord.Data.Extensions;
using OhMyWord.Data.Models;

namespace OhMyWord.Data.Services;

public interface IWordsRepository
{
    Task<IEnumerable<Word>> GetAllWordsAsync(CancellationToken cancellationToken = default);

    Task<(IEnumerable<Word>, int)> GetWordsAsync(
        int offset = WordsRepository.OffsetMinimum,
        int limit = WordsRepository.LimitDefault,
        string filter = "",
        GetWordsOrderBy orderBy = GetWordsOrderBy.Value,
        SortDirection direction = SortDirection.Ascending,
        CancellationToken cancellationToken = default);

    Task<int> GetWordCountAsync(CancellationToken cancellationToken = default);

    Task<Word?> GetWordAsync(PartOfSpeech partOfSpeech, Guid id);
    Task<Word> CreateWordAsync(Word word);
    Task<Word> UpdateWordAsync(Word word);
    Task DeleteWordAsync(PartOfSpeech partOfSpeech, Guid id);
}

public class WordsRepository : Repository<Word>, IWordsRepository
{
    public const int OffsetMinimum = 0;
    public const int LimitDefault = 10;
    public const int LimitMinimum = 1;
    public const int LimitMaximum = 100;

    public WordsRepository(ICosmosDbService cosmosDbService, ILogger<WordsRepository> logger)
        : base(cosmosDbService, logger, "Words")
    {
    }

    public Task<IEnumerable<Word>> GetAllWordsAsync(CancellationToken cancellationToken = default)
        => ReadAllItemsAsync(cancellationToken);

    public async Task<(IEnumerable<Word>, int)> GetWordsAsync(int offset, int limit, string filter,
        GetWordsOrderBy orderBy, SortDirection direction, CancellationToken cancellationToken = default)
    {
        var queryable = GetLinqQueryable<Word>()
            .Where(word => word.Definition.Contains(filter) || word.Value.Contains(filter));

        // get count of words that match the filter
        var countTask = queryable.CountAsync(cancellationToken);

        // apply ordering
        queryable = direction == SortDirection.Descending
            ? orderBy switch
            {
                GetWordsOrderBy.Definition => queryable.OrderByDescending(word => word.Definition),
                GetWordsOrderBy.PartOfSpeech => queryable.OrderByDescending(word => word.PartOfSpeech),
                GetWordsOrderBy.LastModifiedTime => queryable.OrderByDescending(word => word.Timestamp),
                _ => queryable.OrderByDescending(word => word.Value)
            }
            : orderBy switch
            {
                GetWordsOrderBy.Definition => queryable.OrderBy(word => word.Definition),
                GetWordsOrderBy.PartOfSpeech => queryable.OrderBy(word => word.PartOfSpeech),
                GetWordsOrderBy.LastModifiedTime => queryable.OrderBy(word => word.Timestamp),
                _ => queryable.OrderBy(word => word.Value),
            };

        // apply offset and limit
        queryable = queryable.Skip(offset).Take(limit);

        var queryTask = ExecuteQueryAsync(queryable, cancellationToken);

        await Task.WhenAll(queryTask, countTask);

        return (queryTask.Result, countTask.Result);
    }

    public Task<int> GetWordCountAsync(CancellationToken cancellationToken) =>
        GetItemCountAsync(cancellationToken: cancellationToken);

    public Task<Word?> GetWordAsync(PartOfSpeech partOfSpeech, Guid id)
        => ReadItemAsync(id, partOfSpeech.ToPartitionKey());

    public Task<Word> CreateWordAsync(Word word) => CreateItemAsync(word);

    public Task<Word> UpdateWordAsync(Word word) => UpdateItemAsync(word);

    public Task DeleteWordAsync(PartOfSpeech partOfSpeech, Guid id)
        => DeleteItemAsync(id, partOfSpeech.ToPartitionKey());
}
