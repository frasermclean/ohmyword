using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WhatTheWord.Data.Models;
using WhatTheWord.Data.Repositories;

namespace WhatTheWord.Domain.Services;

public class GameCoordinator : BackgroundService
{
    private readonly ILogger<GameCoordinator> logger;
    private readonly IWordsRepository wordsRepository;
    private List<Word> words;

    public GameCoordinator(ILogger<GameCoordinator> logger, IWordsRepository wordsRepository)
    {
        this.logger = logger;
        this.wordsRepository = wordsRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (words is null) words = (await wordsRepository.GetAllWordsAsync()).ToList();
            
            var index = new Random().Next(words.Count);
            var randomWord = words[index];

            logger.LogInformation("Random word: {word}", randomWord);
            await Task.Delay(1000, cancellationToken);
        }
    }
}
