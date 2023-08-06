using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Models;

namespace OhMyWord.Domain.Services;

public interface IWordQueueService
{
    /// <summary>
    /// Total number of words in the database.
    /// </summary>
    int TotalWordCount { get; }

    /// <summary>
    /// Remaining number of words in the queue.
    /// </summary>
    int RemainingWordCount { get; }

    /// <summary>
    /// Get a the next <see cref="Word"/> from a shuffled list of words.
    /// </summary>
    /// <param name="reloadWords">Set to true to reload all words from the database.</param>
    /// <param name="cancellationToken">Task cancellation token</param>
    /// <returns>A random word from the database</returns>
    Task<Word> GetNextWordAsync(bool reloadWords = false, CancellationToken cancellationToken = default);
}

public class WordQueueService : IWordQueueService
{
    private readonly ILogger<WordQueueService> logger;
    private readonly IWordsService wordsService;

    private IReadOnlyList<string>? allWordIds;
    private Queue<string>? shuffledWordIds;

    public WordQueueService(ILogger<WordQueueService> logger, IWordsService wordsService)
    {
        this.logger = logger;
        this.wordsService = wordsService;
    }

    public int TotalWordCount => allWordIds?.Count ?? 0;
    public int RemainingWordCount => shuffledWordIds?.Count ?? 0;

    public async Task<Word> GetNextWordAsync(bool reloadWords, CancellationToken cancellationToken)
    {
        // load all distinct word ids
        if (reloadWords || allWordIds is null)
            allWordIds = await GetAllWordIdsAsync(cancellationToken);

        // shuffle the word ids
        if (reloadWords || shuffledWordIds is null || shuffledWordIds.Count == 0)
        {
            shuffledWordIds = new Queue<string>(ShuffleWordIds(allWordIds));
            logger.LogInformation("Loaded {WordCount} words into the queue", shuffledWordIds.Count);
        }

        // get the next word id from the queue
        if (!shuffledWordIds.TryDequeue(out var wordId))
        {
            logger.LogError("Queue is empty");
            return Word.Default;
        }

        var result = await wordsService.GetWordAsync(wordId, cancellationToken: cancellationToken);
        return result.IsSuccess ? result.Value : Word.Default;
    }

    private async Task<IReadOnlyList<string>> GetAllWordIdsAsync(CancellationToken cancellationToken)
        => await wordsService.GetAllWordIds(cancellationToken).ToListAsync(cancellationToken);

    private static IEnumerable<string> ShuffleWordIds(IEnumerable<string> wordIds)
        => wordIds.OrderBy(_ => Random.Shared.Next());
}
