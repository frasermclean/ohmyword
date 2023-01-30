using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Data.Services;

namespace OhMyWord.Core.Services;

public interface IWordsService
{
    /// <summary>
    /// The number of words that are remaining before a reload.
    /// </summary>
    int RemainingWordCount { get; }

    /// <summary>
    /// Set to true to instruct the service to reload all words from the database
    /// the next time that <see cref="GetNextWordAsync"/> is called.
    /// </summary>
    bool ShouldReloadWords { set; }

    Task<IEnumerable<Word>> GetWordsAsync(int offset = WordsRepository.OffsetMinimum,
        int limit = WordsRepository.LimitDefault, string filter = "", ListWordsOrderBy orderBy = ListWordsOrderBy.Id,
        SortDirection direction = SortDirection.Ascending, CancellationToken cancellationToken = default);

    Task<Word> GetNextWordAsync(CancellationToken cancellationToken = default);
}

public class WordsService : IWordsService
{
    private readonly ILogger<WordsService> logger;
    private readonly IWordsRepository wordsRepository;

    private Stack<Word> shuffledWords = new();

    public WordsService(ILogger<WordsService> logger, IWordsRepository wordsRepository)
    {
        this.logger = logger;
        this.wordsRepository = wordsRepository;
    }

    public int RemainingWordCount => shuffledWords.Count;
    public bool ShouldReloadWords { private get; set; }

    public async Task<IEnumerable<Word>> GetWordsAsync(int offset = WordsRepository.OffsetMinimum,
        int limit = WordsRepository.LimitDefault, string filter = "", ListWordsOrderBy orderBy = ListWordsOrderBy.Id,
        SortDirection direction = SortDirection.Ascending, CancellationToken cancellationToken = default)
    {
        var wordEntities =
            await wordsRepository.GetWordsAsync(offset, limit, filter, orderBy, direction, cancellationToken);

        return wordEntities.Select(entity => new Word() { Id = entity.Id });
    }

    public async Task<Word> GetNextWordAsync(CancellationToken cancellationToken = default)
    {
        // reload words from database
        if (ShouldReloadWords || shuffledWords.Count == 0)
        {
            shuffledWords = await GetShuffledWordsAsync(cancellationToken);
            ShouldReloadWords = false;
        }

        var word = shuffledWords.Pop();
        logger.LogDebug("Randomly selected word: {WordId}", word.Id);

        return word;
    }

    private async Task<Stack<Word>> GetShuffledWordsAsync(CancellationToken cancellationToken)
    {
        return new Stack<Word>();
        // load all words from the database
        // var allWords = new List<Word>(await wordsRepository.GetAllWordsAsync(cancellationToken));
        // if (allWords.Count == 0)
        // {
        //     logger.LogWarning("No words were retrieved from the database!");
        //     allWords.Add(Word.Default);
        // }
        // else
        // {
        //     logger.LogInformation("Retrieved: {Count} words from database", allWords.Count);
        // }
        //
        // // create a stack of randomly shuffled words
        // var stack = new Stack<Word>();
        // var allWordsIndices = new List<int>(Enumerable.Range(0, allWords.Count));
        // while (allWordsIndices.Count > 0)
        // {
        //     var index = Random.Shared.Next(allWordsIndices.Count);
        //     stack.Push(allWords[allWordsIndices[index]]);
        //     allWordsIndices.RemoveAt(index);
        // }
        //
        // return stack;
    }
}
