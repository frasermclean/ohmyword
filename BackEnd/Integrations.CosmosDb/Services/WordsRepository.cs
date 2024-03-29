﻿using FluentResults;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Integrations.CosmosDb.Models.Entities;
using OhMyWord.Integrations.CosmosDb.Options;

namespace OhMyWord.Integrations.CosmosDb.Services;

public interface IWordsRepository
{
    IAsyncEnumerable<WordEntity> GetAllWordsAsync(CancellationToken cancellationToken = default);

    IAsyncEnumerable<WordEntity> SearchWords(
        int offset = WordsRepository.OffsetMinimum,
        int limit = WordsRepository.LimitDefault,
        string filter = "",
        string orderBy = "",
        bool isDescending = false,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> GetAllWordIds(CancellationToken cancellationToken = default);

    Task<int> GetTotalWordCountAsync(CancellationToken cancellationToken = default);

    Task<Result<WordEntity>> GetWordAsync(string id, CancellationToken cancellationToken = default);
    Task<Result<WordEntity>> CreateWordAsync(WordEntity entity, CancellationToken cancellationToken = default);
    Task<Result<WordEntity>> UpdateWordAsync(WordEntity entity, CancellationToken cancellationToken = default);
    Task<Result> DeleteWordAsync(string wordId, CancellationToken cancellationToken = default);
}

public class WordsRepository : Repository<WordEntity>, IWordsRepository
{
    public const int OffsetMinimum = 0;
    public const int LimitDefault = 10;
    public const int LimitMinimum = 1;
    public const int LimitMaximum = 100;

    public WordsRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> options,
        ILogger<WordsRepository> logger)
        : base(cosmosClient, logger, options.Value.DatabaseId, "words")
    {
    }

    public IAsyncEnumerable<WordEntity> GetAllWordsAsync(CancellationToken cancellationToken = default)
        => ReadPartitionItems(null, cancellationToken);

    public IAsyncEnumerable<WordEntity> SearchWords(int offset, int limit, string filter, string orderBy,
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

        return ExecuteQuery<WordEntity>(queryDefinition, maxItemCount: limit, cancellationToken: cancellationToken);
    }

    public IAsyncEnumerable<string> GetAllWordIds(CancellationToken cancellationToken = default) =>
        ReadItemIds(null, cancellationToken);

    public Task<int> GetTotalWordCountAsync(CancellationToken cancellationToken) =>
        GetItemCountAsync(cancellationToken: cancellationToken);

    public Task<Result<WordEntity>> GetWordAsync(string id, CancellationToken cancellationToken) =>
        ReadItemAsync(id, id, cancellationToken: cancellationToken);

    public Task<Result<WordEntity>> CreateWordAsync(WordEntity entity, CancellationToken cancellationToken) =>
        CreateItemAsync(entity, cancellationToken);

    public Task<Result<WordEntity>> UpdateWordAsync(WordEntity entity, CancellationToken cancellationToken) =>
        ReplaceItemAsync(entity, cancellationToken);

    public Task<Result> DeleteWordAsync(string wordId, CancellationToken cancellationToken) =>
        DeleteItemAsync(wordId, wordId, cancellationToken);
    
    public static readonly IEnumerable<string> ValidOrderByValues = new[]
    {
        "id",
        "lastModifiedTime",
        "length"
    }; 
}
