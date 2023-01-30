using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using OhMyWord.Data.Entities;


namespace OhMyWord.Data.Services;

public interface IWordsRepository
{
    IAsyncEnumerable<WordEntity> GetAllWordsAsync(CancellationToken cancellationToken = default);

    IAsyncEnumerable<WordEntity> ListWords(
        int offset = WordsRepository.OffsetMinimum,
        int limit = WordsRepository.LimitDefault,
        string filter = "",
        ListWordsOrderBy orderBy = ListWordsOrderBy.Id,
        SortDirection direction = SortDirection.Ascending,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> GetAllWordIds(CancellationToken cancellationToken = default);

    Task<int> GetWordCountAsync(CancellationToken cancellationToken = default);

    Task<WordEntity?> GetWordAsync(string id, CancellationToken cancellationToken = default);
    Task CreateWordAsync(WordEntity entity, CancellationToken cancellationToken = default);
    Task UpdateWordAsync(WordEntity entity, CancellationToken cancellationToken = default);
    Task DeleteWordAsync(string wordId, CancellationToken cancellationToken = default);
}

public class WordsRepository : Repository<WordEntity>, IWordsRepository
{
    public const int OffsetMinimum = 0;
    public const int LimitDefault = 10;
    public const int LimitMinimum = 1;
    public const int LimitMaximum = 100;

    public WordsRepository(IDatabaseService databaseService, ILogger<WordsRepository> logger)
        : base(databaseService, logger, "words")
    {
    }

    public IAsyncEnumerable<WordEntity> GetAllWordsAsync(CancellationToken cancellationToken = default)
        => ReadPartitionItems(null, cancellationToken);

    public IAsyncEnumerable<WordEntity> ListWords(int offset, int limit, string filter,
        ListWordsOrderBy orderBy, SortDirection direction, CancellationToken cancellationToken = default)
    {
        var orderByString = orderBy switch
        {
            ListWordsOrderBy.LastModifiedTime => "word._ts",
            ListWordsOrderBy.Length => "word.id.length",
            _ => "word.id"
        };

        var directionString = direction == SortDirection.Ascending ? "ASC" : "DESC";

        var queryDefinition = new QueryDefinition($"""
            SELECT * FROM word
            WHERE (CONTAINS(word["id"], LOWER(@filter)))
            ORDER BY {orderByString} {directionString}
            OFFSET @offset LIMIT @limit
            """)
            .WithParameter("@filter", filter)
            .WithParameter("@offset", offset)
            .WithParameter("@limit", limit);

        return ExecuteQuery<WordEntity>(queryDefinition, cancellationToken: cancellationToken);
    }

    public IAsyncEnumerable<string> GetAllWordIds(CancellationToken cancellationToken = default) =>
        ReadItemIds(null, cancellationToken);

    public Task<int> GetWordCountAsync(CancellationToken cancellationToken) =>
        GetItemCountAsync(cancellationToken: cancellationToken);

    public Task<WordEntity?> GetWordAsync(string id, CancellationToken cancellationToken) =>
        ReadItemAsync(id, id, cancellationToken: cancellationToken);

    public Task CreateWordAsync(WordEntity entity, CancellationToken cancellationToken) =>
        CreateItemAsync(entity, cancellationToken);

    public Task UpdateWordAsync(WordEntity entity, CancellationToken cancellationToken) =>
        UpdateItemAsync(entity, cancellationToken);

    public Task DeleteWordAsync(string wordId, CancellationToken cancellationToken) =>
        DeleteItemAsync(wordId, wordId, cancellationToken);
}

public enum ListWordsOrderBy
{
    Id,
    Length,
    LastModifiedTime,
}
