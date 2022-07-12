using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Services.Data.Repositories;

namespace OhMyWord.Services.Game;

public interface IWordsService
{
    Task<Word> SelectRandomWordAsync(CancellationToken cancellationToken);
}

public class WordsService : IWordsService
{
    private readonly ILogger<WordsService> logger;
    private readonly IWordsRepository wordsRepository;
    private readonly List<Word> words = new();
    private readonly List<int> previousIndices = new();

    public WordsService(ILogger<WordsService> logger, IWordsRepository wordsRepository)
    {
        this.logger = logger;
        this.wordsRepository = wordsRepository;
    }

    public async Task<Word> SelectRandomWordAsync(CancellationToken cancellationToken)
    {
        if (words.Count == 0) await LoadWordsFromRepositoryAsync(cancellationToken);

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
