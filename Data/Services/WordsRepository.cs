using Microsoft.Extensions.Logging;
using OhMyWord.Data.Extensions;
using OhMyWord.Data.Models;

namespace OhMyWord.Data.Services;

public interface IWordsRepository
{
    Task<IEnumerable<Word>> GetAllWordsAsync(CancellationToken cancellationToken = default);
    Task<Word?> GetWordAsync(PartOfSpeech partOfSpeech, Guid id);
    Task<Word> CreateWordAsync(Word word);
    Task<Word> UpdateWordAsync(Word word);
    Task DeleteWordAsync(PartOfSpeech partOfSpeech, Guid id);
}

public class WordsRepository : Repository<Word>, IWordsRepository
{
    public WordsRepository(ICosmosDbService cosmosDbService, ILogger<WordsRepository> logger)
        : base(cosmosDbService, logger, "Words") { }

    public Task<IEnumerable<Word>> GetAllWordsAsync(CancellationToken cancellationToken = default)
        => ReadAllItemsAsync(cancellationToken);

    public Task<Word?> GetWordAsync(PartOfSpeech partOfSpeech, Guid id)
        => ReadItemAsync(id, partOfSpeech.ToPartitionKey());

    public Task<Word> CreateWordAsync(Word word) => CreateItemAsync(word);

    public Task<Word> UpdateWordAsync(Word word) => UpdateItemAsync(word);

    public Task DeleteWordAsync(PartOfSpeech partOfSpeech, Guid id)
        => DeleteItemAsync(id, partOfSpeech.ToPartitionKey());
}
