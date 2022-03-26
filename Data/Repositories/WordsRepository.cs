using Microsoft.Extensions.Logging;
using OhMyWord.Data.Models;

namespace OhMyWord.Data.Repositories;

public interface IWordsRepository
{
    Task<IEnumerable<Word>> GetAllWordsAsync();
}

public class WordsRepository : Repository<Word>, IWordsRepository
{
    public WordsRepository(ICosmosDbService cosmosDbService, ILogger<WordsRepository> logger)
        : base(cosmosDbService, logger, "Words")
    {
    }

    public async Task<IEnumerable<Word>> GetAllWordsAsync()
    {
        return await ReadAllItemsAsync();
    }
}
