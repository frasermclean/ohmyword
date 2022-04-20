using Microsoft.Extensions.Logging;
using OhMyWord.Core.Extensions;
using OhMyWord.Core.Models;

namespace OhMyWord.Services.Data.Repositories;

public interface IWordsRepository
{
    Task<IEnumerable<Word>> GetAllWordsAsync();
    Task<RepositoryActionResult<Word>> GetWordByValueAsync(PartOfSpeech partOfSpeech, string value);
    Task<RepositoryActionResult<Word>> CreateWordAsync(Word word);
    Task<RepositoryActionResult<Word>> UpdateWordAsync(PartOfSpeech partOfSpeech, string value, Word word);
    Task<RepositoryActionResult<Word>> DeleteWordAsync(PartOfSpeech partOfSpeech, string value);
}

public class WordsRepository : Repository<Word>, IWordsRepository
{
    public WordsRepository(ICosmosDbService cosmosDbService, ILogger<WordsRepository> logger)
        : base(cosmosDbService, logger, "Words") { }

    public Task<IEnumerable<Word>> GetAllWordsAsync() => ReadAllItemsAsync();

    public Task<RepositoryActionResult<Word>> GetWordByValueAsync(PartOfSpeech partOfSpeech, string value) =>
        ReadItemAsync(value, partOfSpeech.ToPartitionKey());

    public Task<RepositoryActionResult<Word>> CreateWordAsync(Word word) => CreateItemAsync(word);

    public Task<RepositoryActionResult<Word>> UpdateWordAsync(PartOfSpeech partOfSpeech, string value, Word word) =>
        UpdateItemAsync(word, value, partOfSpeech.ToPartitionKey());

    public Task<RepositoryActionResult<Word>> DeleteWordAsync(PartOfSpeech partOfSpeech, string value) =>
        DeleteItemAsync(value, partOfSpeech.ToPartitionKey());
}
