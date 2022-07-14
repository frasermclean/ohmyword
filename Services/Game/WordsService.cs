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

    Task<Word> SelectRandomWordAsync(CancellationToken cancellationToken);
}

public class WordsService : IWordsService
{
    private readonly ILogger<WordsService> logger;
    private readonly IWordsRepository wordsRepository;
    private readonly List<Word> words = new();
    private readonly List<int> previousIndices = new();

    public bool ShouldReloadWords { get; set; } = true;

    public WordsService(ILogger<WordsService> logger, IWordsRepository wordsRepository)
    {
        this.logger = logger;
        this.wordsRepository = wordsRepository;
    }

    public async Task<Word> SelectRandomWordAsync(CancellationToken cancellationToken)
    {
        // check if we should load words from the database
        if (ShouldReloadWords)
        {
            await LoadWordsFromRepositoryAsync(cancellationToken);
            ShouldReloadWords = false;
        }

        // we've gone through all words so start again
        if (previousIndices.Count == words.Count)
            previousIndices.Clear();

        int index;
        do index = Random.Shared.Next(words.Count);
        while (previousIndices.Contains(index));

        var randomWord = words[index];
        previousIndices.Add(index);

        logger.LogDebug("Randomly selected word: {word}. Previous indices count: {count}", randomWord, previousIndices.Count);

        return randomWord;
    }

    private async Task LoadWordsFromRepositoryAsync(CancellationToken cancellationToken)
    {
        words.Clear();
        words.AddRange(await wordsRepository.GetAllWordsAsync(cancellationToken));

        if (words.Count == 0)
        {
            logger.LogWarning("No words were retrieved from the database!");
            words.Add(Word.Default);
        }
        else
            logger.LogInformation("Retrieved: {count} words from database.", words.Count);
    }
}
