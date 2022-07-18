using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Services.Data.Repositories;

namespace OhMyWord.Services.Game;

public interface IWordsService
{
    /// <summary>
    /// The number of words that are remaining before a reload.
    /// </summary>
    int RemainingWordCount { get; }

    /// <summary>
    /// Set to true to instruct the service to reload all words from the database
    /// the next time that SelectRandomWordAsync is called.
    /// </summary>
    bool ShouldReloadWords { set; }

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

    public async Task<Word> GetNextWordAsync(CancellationToken cancellationToken = default)
    {
        // reload words from database
        if (ShouldReloadWords || shuffledWords.Count == 0)
        {
            shuffledWords = await GetShuffledWordsAsync(cancellationToken);
            ShouldReloadWords = false;
        }

        var word = shuffledWords.Pop();
        logger.LogDebug("Randomly selected word: {word}.", word);

        return word;
    }

    private async Task<Stack<Word>> GetShuffledWordsAsync(CancellationToken cancellationToken)
    {
        // load all words from the database
        var allWords = new List<Word>(await wordsRepository.GetAllWordsAsync(cancellationToken));
        if (allWords.Count == 0)
        {
            logger.LogWarning("No words were retrieved from the database!");
            allWords.Add(Word.Default);
        }
        else
        {
            logger.LogInformation("Retrieved: {count} words from database.", allWords.Count);
        }

        // create a stack of randomly shuffled words
        var stack = new Stack<Word>();
        var previousIndices = new List<int>();
        while (stack.Count < allWords.Count)
        {
            var index = Random.Shared.Next(allWords.Count);
            if (previousIndices.Contains(index)) continue;
            previousIndices.Add(index);
            stack.Push(allWords[index]);
        }

        return stack;
    }
}