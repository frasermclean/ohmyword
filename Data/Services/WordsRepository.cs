﻿using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
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
    public const int LimitDefault = 20;
    public const int LimitMinimum = 1;
    public const int LimitMaximum = 100;

    public WordsRepository(ICosmosDbService cosmosDbService, ILogger<WordsRepository> logger)
        : base(cosmosDbService, logger, "Words")
    {
    }

    public Task<IEnumerable<Word>> GetAllWordsAsync(CancellationToken cancellationToken = default)
        => ReadAllItemsAsync(cancellationToken);

    public Task<IEnumerable<Word>> GetWordsAsync(int offset, int limit, string? filter,
        CancellationToken cancellationToken = default)
    {
        var queryable = GetLinqQueryable<Word>();

        // apply filter if it is defined
        if (!string.IsNullOrEmpty(filter))
            queryable = queryable.Where(word => word.Definition.Contains(filter) || word.Value.Contains(filter));

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
