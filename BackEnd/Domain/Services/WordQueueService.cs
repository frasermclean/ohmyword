using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Models;

namespace OhMyWord.Domain.Services;

public interface IWordQueueService
{
    int RemainingWordCount { get; }

    /// <summary>
    /// Get a the next <see cref="Word"/> from a shuffled list of words.
    /// </summary>
    /// <param name="cancellationToken">Task cancellation token</param>
    /// <returns>A random word from the database</returns>
    Task<Word> GetNextWordAsync(CancellationToken cancellationToken = default);
}

public class WordQueueService : IWordQueueService
{
    private readonly ILogger<WordQueueService> logger;
    private readonly IWordsService wordsService;

    private Stack<string> shuffledWordIds = new();

    public WordQueueService(ILogger<WordQueueService> logger, IWordsService wordsService)
    {
        this.logger = logger;
        this.wordsService = wordsService;
    }

    public int RemainingWordCount => shuffledWordIds.Count;

    public async Task<Word> GetNextWordAsync(CancellationToken cancellationToken = default)
    {
        if (shuffledWordIds.Count == 0)
        {
            shuffledWordIds = await GetShuffledWordIdsAsync(cancellationToken);
            logger.LogInformation("Loaded {WordCount} words into the queue", shuffledWordIds.Count);
        }

        string wordId;
        try
        {
            wordId = shuffledWordIds.Pop();
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(exception, "No word ID to pop from queue");
            return Word.Default;
        }

        var result = await wordsService.GetWordAsync(wordId, cancellationToken);

        return result.Match(
            word => word,
            _ => Word.Default);
    }

    private async Task<Stack<string>> GetShuffledWordIdsAsync(CancellationToken cancellationToken)
    {
        var wordIds = await wordsService.GetAllWordIds(cancellationToken)
            .OrderBy(_ => Random.Shared.Next())
            .ToListAsync(cancellationToken);

        return new Stack<string>(wordIds);
    }
}
