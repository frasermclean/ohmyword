﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WhatTheWord.Data.Models;
using WhatTheWord.Data.Repositories;

namespace WhatTheWord.Domain.Services;

public interface IGameService
{
    Task<(Word, DateTime)> GetCurrentWord();
}

public class GameService : IGameService
{
    private readonly ILogger<GameService> logger;
    private readonly IWordsRepository wordsRepository;

    private List<Word> allWords = new();
    private Word? currentWord;
    private DateTime currentWordExpiry;

    private GameServiceOptions Options { get; }

    public GameService(IOptions<GameServiceOptions> options, ILogger<GameService> logger, IWordsRepository wordsRepository)
    {
        this.logger = logger;
        this.wordsRepository = wordsRepository;
        Options = options.Value;
    }

    public async Task<(Word, DateTime)> GetCurrentWord()
    {
        var now = DateTime.UtcNow;

        // early exit if current word is still valid
        if (currentWord is not null && now < currentWordExpiry)
            return (currentWord, currentWordExpiry);

        // request new list of words from repository
        if (allWords.Count == 0)
        {
            logger.LogInformation("Requesting all words from repository.");
            allWords = new List<Word>(await wordsRepository.GetAllWordsAsync());

            if (allWords.Count == 0)
            {
                logger.LogError("No words are available to select from!");
                throw new ApplicationException("No words are available to select from!");
            }
        }

        // set current word to randomly selected one
        if (currentWord is null || now > currentWordExpiry)
        {
            var index = Random.Shared.Next(0, allWords.Count);
            currentWord = allWords[index];
            currentWordExpiry = DateTime.UtcNow.AddSeconds(Options.RoundLength);
            allWords.Remove(currentWord);

            logger.LogInformation("Current word selected as: {word}", currentWord);
        }

        return (currentWord, currentWordExpiry);
    }
}
