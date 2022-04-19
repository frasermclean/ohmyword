using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Services.Extensions;

namespace OhMyWord.Services.Data.Repositories;

public interface IWordsRepository
{
    Task<IEnumerable<Word>> GetAllWordsAsync();
    Task<RepositoryActionResult<Word>> GetWordByValueAsync(PartOfSpeech partOfSpeech, string value);
    Task<Word> CreateWordAsync(Word word);
    Task<RepositoryActionResult<Word>> UpdateWordAsync(PartOfSpeech partOfSpeech, string value, Word word);
    Task<bool> DeleteWordAsync(PartOfSpeech partOfSpeech, string value);
}

public class WordsRepository : Repository<Word>, IWordsRepository
{
    public WordsRepository(ICosmosDbService cosmosDbService, ILogger<WordsRepository> logger)
        : base(cosmosDbService, logger, ContainerId.Words) { }

    public Task<IEnumerable<Word>> GetAllWordsAsync() => ReadAllItemsAsync();

    public Task<RepositoryActionResult<Word>> GetWordByValueAsync(PartOfSpeech partOfSpeech, string value) =>
        ReadItemAsync(value, partOfSpeech.ToPartitionKey());

    public Task<Word> CreateWordAsync(Word word) => CreateItemAsync(word);

    public Task<RepositoryActionResult<Word>> UpdateWordAsync(PartOfSpeech partOfSpeech, string value, Word word) =>
        UpdateItemAsync(word, value, partOfSpeech.ToPartitionKey());

    public Task<bool> DeleteWordAsync(PartOfSpeech partOfSpeech, string value) =>
        DeleteItemAsync(value, partOfSpeech.ToPartitionKey());
}
