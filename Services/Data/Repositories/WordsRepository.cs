using Microsoft.Extensions.Logging;
using OhMyWord.Core.Extensions;
using OhMyWord.Core.Models;

namespace OhMyWord.Services.Data.Repositories;

public interface IWordsRepository
{
    Task<IEnumerable<Word>> GetAllWordsAsync(CancellationToken cancellationToken = default);
    Task<RepositoryActionResult<Word>> GetWordByValueAsync(PartOfSpeech partOfSpeech, string value);
    Task<RepositoryActionResult<Word>> CreateWordAsync(Word word);
    Task<RepositoryActionResult<Word>> UpdateWordAsync(Word word);
    Task<RepositoryActionResult<Word>> DeleteWordAsync(PartOfSpeech partOfSpeech, string value);
}

public class WordsRepository : Repository<Word>, IWordsRepository
{
    public WordsRepository(ICosmosDbService cosmosDbService, ILogger<WordsRepository> logger)
        : base(cosmosDbService, logger, "Words", "/partOfSpeech") { }

    public Task<IEnumerable<Word>> GetAllWordsAsync(CancellationToken cancellationToken = default)
        => ReadAllItemsAsync(cancellationToken);

    public Task<RepositoryActionResult<Word>> GetWordByValueAsync(PartOfSpeech partOfSpeech, string value)
        => ReadItemAsync(value, partOfSpeech.ToPartitionKey());

    public Task<RepositoryActionResult<Word>> CreateWordAsync(Word word) => CreateItemAsync(word);

    public Task<RepositoryActionResult<Word>> UpdateWordAsync(Word word) => UpdateItemAsync(word);

    public Task<RepositoryActionResult<Word>> DeleteWordAsync(PartOfSpeech partOfSpeech, string value)
        => DeleteItemAsync(value, partOfSpeech.ToPartitionKey());
}
