using Microsoft.Extensions.Logging;
using OhMyWord.Data.Extensions;
using OhMyWord.Data.Models;

namespace OhMyWord.Data.Services;

public interface IWordsRepository
{
    Task<IEnumerable<Word>> GetAllWordsAsync(CancellationToken cancellationToken = default);
    Task<RepositoryActionResult<Word>> GetWordAsync(PartOfSpeech partOfSpeech, Guid id);
    Task<RepositoryActionResult<Word>> CreateWordAsync(Word word);
    Task<RepositoryActionResult<Word>> UpdateWordAsync(Word word);
    Task<RepositoryActionResult<Word>> DeleteWordAsync(PartOfSpeech partOfSpeech, Guid id);
}

public class WordsRepository : Repository<Word>, IWordsRepository
{
    public WordsRepository(ICosmosDbService cosmosDbService, ILogger<WordsRepository> logger)
        : base(cosmosDbService, logger, "Words", "/partOfSpeech") { }

    public Task<IEnumerable<Word>> GetAllWordsAsync(CancellationToken cancellationToken = default)
        => ReadAllItemsAsync(cancellationToken);

    public Task<RepositoryActionResult<Word>> GetWordAsync(PartOfSpeech partOfSpeech, Guid id)
        => ReadItemAsync(id, partOfSpeech.ToPartitionKey());

    public Task<RepositoryActionResult<Word>> CreateWordAsync(Word word) => CreateItemAsync(word);

    public Task<RepositoryActionResult<Word>> UpdateWordAsync(Word word) => UpdateItemAsync(word);

    public Task<RepositoryActionResult<Word>> DeleteWordAsync(PartOfSpeech partOfSpeech, Guid id)
        => DeleteItemAsync(id, partOfSpeech.ToPartitionKey());
}
