using Microsoft.Extensions.Logging;
using OhMyWord.Data.Extensions;
using OhMyWord.Data.Models;

namespace OhMyWord.Data.Services;

public interface IWordsRepository
{
    Task<IEnumerable<Word>> GetAllWordsAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<Word>> GetWordsAsync(
        int offset = WordsRepository.OffsetMinimum,
        int limit = WordsRepository.LimitDefault,
        string? filter = null,
        string? orderBy = null,
        bool desc = false,
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

    public Task<IEnumerable<Word>> GetWordsAsync(int offset, int limit, string? filter, string? orderBy, bool desc,
        CancellationToken cancellationToken = default)
    {
        var queryable = GetLinqQueryable<Word>();

        // apply filter if it is defined
        if (!string.IsNullOrEmpty(filter))
            queryable = queryable.Where(word => word.Definition.Contains(filter) || word.Value.Contains(filter));

        // apply ordering
        if (orderBy is not null)
            queryable = desc
                ? orderBy switch
                {
                    "value" => queryable.OrderByDescending(word => word.Value),
                    "definition" => queryable.OrderByDescending(word => word.Definition),
                    "partOfSpeech" => queryable.OrderByDescending(word => word.PartOfSpeech),
                    "lastModifiedTime" => queryable.OrderByDescending(word => word.Timestamp),
                    _ => queryable
                }
                : orderBy switch
                {
                    "value" => queryable.OrderBy(word => word.Value),
                    "definition" => queryable.OrderBy(word => word.Definition),
                    "partOfSpeech" => queryable.OrderBy(word => word.PartOfSpeech),
                    "lastModifiedTime" => queryable.OrderBy(word => word.Timestamp),
                    _ => queryable
                };

        // apply offset and limit
        queryable = queryable.Skip(offset).Take(limit);

        return ExecuteQueryAsync(queryable, cancellationToken);
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
