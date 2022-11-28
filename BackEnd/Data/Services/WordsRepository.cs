using Microsoft.Azure.Cosmos;
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
        var orderByClause = GetOrderByClause(orderBy, direction);
        var queryDefinition = new QueryDefinition($"""
            SELECT * FROM word
            WHERE 
                (CONTAINS(word["value"], LOWER(@filter))) OR 
                (CONTAINS(word["definition"], LOWER(@filter)))
            {orderByClause}  
            OFFSET @offset LIMIT @limit
            """ )
            .WithParameter("@filter", filter)
            .WithParameter("@offset", offset)
            .WithParameter("@limit", limit);

        var words = await ExecuteQueryAsync<Word>(queryDefinition, cancellationToken: cancellationToken);

        return (words, words.Count);
    }
    
    private static string GetOrderByClause(GetWordsOrderBy orderBy, SortDirection direction)
    {
        var orderByString = orderBy switch
        {
            GetWordsOrderBy.Definition => "word.definition",
            GetWordsOrderBy.PartOfSpeech => "word.partOfSpeech",
            GetWordsOrderBy.LastModifiedTime => "word._ts",
            _ => "word[\"value\"]"
        };

        var directionString = direction == SortDirection.Ascending ? "ASC" : "DESC";

        return $"ORDER BY {orderByString} {directionString}";
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

public enum GetWordsOrderBy
{
    Value,
    PartOfSpeech,
    Definition,
    LastModifiedTime,
}
