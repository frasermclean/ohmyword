﻿using Microsoft.AspNetCore.SignalR;
using WhatTheWord.Api.Hubs;
using WhatTheWord.Data.Models;
using WhatTheWord.Data.Repositories;
using WhatTheWord.Domain.Responses.Game;

namespace WhatTheWord.Api.Services;

public class GameCoordinator : BackgroundService
{
    private readonly ILogger<GameCoordinator> logger;
    private readonly IWordsRepository wordsRepository;
    private readonly IHubContext<GameHub, IGameHub> gameHubContext;

    private List<Word>? words;

    private const int DelaySeconds = 5;

    public GameCoordinator(ILogger<GameCoordinator> logger, IWordsRepository wordsRepository, IHubContext<GameHub, IGameHub> gameHubContext)
    {
        this.logger = logger;
        this.wordsRepository = wordsRepository;
        this.gameHubContext = gameHubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // request list of words from repository
            words ??= (await wordsRepository.GetAllWordsAsync()).ToList();
            
            // remove a random word from the list
            var index = new Random().Next(words.Count);
            var randomWord = words[index];
            words.Remove(randomWord);

            await gameHubContext.Clients.All.SendHint(new HintResponse(randomWord, DateTime.UtcNow.AddSeconds(DelaySeconds)));

            // reset list to null when all words have been selected
            if (words.Count == 0) words = null;

            logger.LogInformation("Random word: {word}", randomWord);
            await Task.Delay(DelaySeconds * 1000, cancellationToken);
        }
    }
}
