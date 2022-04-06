using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;

namespace OhMyWord.Services.Data.Repositories;

public interface IWordsRepository
{
    Task<IEnumerable<Word>> GetAllWordsAsync();
    Task<Word?> GetWordById(string wordId);
    Task<Word> CreateWordAsync(Word word);
    Task<bool> DeleteWordAsync(string wordId);
}

public class WordsRepository : Repository<Word>, IWordsRepository
{
    public WordsRepository(ICosmosDbService cosmosDbService, ILogger<WordsRepository> logger)
        : base(cosmosDbService, logger, ContainerId.Words)
    {
    }

    public Task<IEnumerable<Word>> GetAllWordsAsync() => ReadAllItemsAsync();
    public Task<Word?> GetWordById(string wordId) => ReadItemAsync(wordId, wordId);
    public Task<Word> CreateWordAsync(Word word) => CreateItemAsync(word);
    public Task<bool> DeleteWordAsync(string wordId) => DeleteItemAsync(wordId, wordId);
}
