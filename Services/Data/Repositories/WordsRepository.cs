using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Services.Extensions;

namespace OhMyWord.Services.Data.Repositories;

public interface IWordsRepository
{
    Task<IEnumerable<Word>> GetAllWordsAsync();
    Task<Word?> GetWordByValueAsync(PartOfSpeech partOfSpeech, string value);
    Task<Word> CreateWordAsync(Word word);
    Task<bool> DeleteWordAsync(PartOfSpeech partOfSpeech, string value);
}

public class WordsRepository : Repository<Word>, IWordsRepository
{
    public WordsRepository(ICosmosDbService cosmosDbService, ILogger<WordsRepository> logger)
        : base(cosmosDbService, logger, ContainerId.Words)
    {
    }

    public Task<IEnumerable<Word>> GetAllWordsAsync() => ReadAllItemsAsync();

    public Task<Word?> GetWordByValueAsync(PartOfSpeech partOfSpeech, string value) =>
        ReadItemAsync(value, partOfSpeech.ToPartitionKey());

    public Task<Word> CreateWordAsync(Word word) => CreateItemAsync(word);

    public Task<bool> DeleteWordAsync(PartOfSpeech partOfSpeech, string value) =>
        DeleteItemAsync(value, partOfSpeech.ToPartitionKey());
}
