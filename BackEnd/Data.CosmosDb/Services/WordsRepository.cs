using AutoMapper;
using FluentResults;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using OhMyWord.Data.CosmosDb.Models;
using OhMyWord.Data.CosmosDb.Options;

namespace OhMyWord.Data.CosmosDb.Services;

public class WordsRepository : Repository<WordItem>, IWordsRepository
{
    private readonly IMapper mapper;

    public WordsRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options,
        ILogger<WordsRepository> logger, IMapper mapper)
        : base(cosmosClient, logger, options.Value.DatabaseId, "words")
    {
        this.mapper = mapper;
    }

    public IAsyncEnumerable<Word> GetAllWordsAsync(CancellationToken cancellationToken = default)
        => ReadPartitionItems(null, cancellationToken).Select(mapper.Map<Word>);

    public IAsyncEnumerable<Word> SearchWords(int offset, int limit, string filter, string orderBy,
        bool isDescending, CancellationToken cancellationToken = default)
    {
        var orderByString = orderBy switch
        {
            "lastModifiedTime" => "word._ts",
            "length" => "word.id.length",
            _ => "word.id"
        };

        var directionString = isDescending ? "DESC" : "ASC";

        var queryDefinition = new QueryDefinition($"""
                                                   SELECT * FROM word
                                                   WHERE (CONTAINS(word["id"], LOWER(@filter)))
                                                   ORDER BY {orderByString} {directionString}
                                                   OFFSET @offset LIMIT @limit
                                                   """)
            .WithParameter("@filter", filter)
            .WithParameter("@offset", offset)
            .WithParameter("@limit", limit);

        return ExecuteQuery<WordItem>(queryDefinition, maxItemCount: limit, cancellationToken: cancellationToken)
            .Select(mapper.Map<Word>);
    }

    public IAsyncEnumerable<string> GetAllWordIds(CancellationToken cancellationToken = default) =>
        ReadItemIds(null, cancellationToken);

    public Task<int> GetTotalWordCountAsync(CancellationToken cancellationToken) =>
        GetItemCountAsync(cancellationToken: cancellationToken);

    public async Task<Result<Word>> GetWordAsync(string id, CancellationToken cancellationToken)
    {
        var result = await ReadItemAsync(id, id, cancellationToken: cancellationToken);
        return result.Map(mapper.Map<Word>);
    }

    public async Task<Result<Word>> CreateWordAsync(Word word, CancellationToken cancellationToken)
    {
        var item = mapper.Map<WordItem>(word);
        var result = await CreateItemAsync(item, cancellationToken);
        return result.Map(mapper.Map<Word>);
    }

    public async Task<Result<Word>> UpdateWordAsync(Word word, CancellationToken cancellationToken)
    {
        var item = mapper.Map<WordItem>(word);
        var result = await ReplaceItemAsync(item, cancellationToken);
        return result.Map(mapper.Map<Word>);
    }

    public Task<Result> DeleteWordAsync(string wordId, CancellationToken cancellationToken) =>
        DeleteItemAsync(wordId, wordId, cancellationToken);

    public static readonly IEnumerable<string> ValidOrderByValues = new[] { "id", "lastModifiedTime", "length" };
}
