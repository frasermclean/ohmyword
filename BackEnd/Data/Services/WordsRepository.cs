using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Data.Entities;
using OhMyWord.Data.Options;


namespace OhMyWord.Data.Services;

public interface IWordsRepository
{
    IAsyncEnumerable<WordEntity> GetAllWordsAsync(CancellationToken cancellationToken = default);

    IAsyncEnumerable<WordEntity> SearchWords(
        int offset = WordsRepository.OffsetMinimum,
        int limit = WordsRepository.LimitDefault,
        string filter = "",
        SearchWordsOrderBy orderBy = SearchWordsOrderBy.Id,
        SortDirection direction = SortDirection.Ascending,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> GetAllWordIds(CancellationToken cancellationToken = default);

    Task<int> GetTotalWordCountAsync(CancellationToken cancellationToken = default);

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

    public WordsRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options,
        ILogger<WordsRepository> logger)
        : base(cosmosClient, options, logger, "words")
    {
    }

    public IAsyncEnumerable<WordEntity> GetAllWordsAsync(CancellationToken cancellationToken = default)
        => ReadPartitionItems(null, cancellationToken);

    public IAsyncEnumerable<WordEntity> SearchWords(int offset, int limit, string filter,
        SearchWordsOrderBy orderBy, SortDirection direction, CancellationToken cancellationToken = default)
    {
        var orderByString = orderBy switch
        {
            SearchWordsOrderBy.LastModifiedTime => "word._ts",
            SearchWordsOrderBy.Length => "word.id.length",
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

        return ExecuteQuery<WordEntity>(queryDefinition, maxItemCount: limit, cancellationToken: cancellationToken);
    }

    public IAsyncEnumerable<string> GetAllWordIds(CancellationToken cancellationToken = default) =>
        ReadItemIds(null, cancellationToken);

    public Task<int> GetTotalWordCountAsync(CancellationToken cancellationToken) =>
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

public enum SearchWordsOrderBy
{
    Id,
    Length,
    LastModifiedTime,
}
