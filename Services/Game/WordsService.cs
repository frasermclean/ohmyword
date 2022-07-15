using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Services.Data.Repositories;

namespace OhMyWord.Services.Game;

public interface IWordsService
{
    /// <summary>
    /// Set to true to instruct the service to reload all words from the database
    /// the next time that SelectRandomWordAsync is called.
    /// </summary>
    bool ShouldReloadWords { get; set; }

    Task<Word> GetNextWordAsync(CancellationToken cancellationToken);
}

public class WordsService : IWordsService
{
    private readonly ILogger<WordsService> logger;
    private readonly IWordsRepository wordsRepository;

    private Stack<Word> shuffledWords = new();

    public bool ShouldReloadWords { get; set; }

    public WordsService(ILogger<WordsService> logger, IWordsRepository wordsRepository)
    {
        this.logger = logger;
        this.wordsRepository = wordsRepository;
    }

    public async Task<Word> GetNextWordAsync(CancellationToken cancellationToken)
    {
        // reload words from database
        if (shuffledWords.Count == 0 || ShouldReloadWords)
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
            logger.LogInformation("Retrieved: {count} words from database.", allWords.Count);

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