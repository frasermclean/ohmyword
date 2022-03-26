﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Api.Hubs;
using OhMyWord.Api.Mediator.Requests.Game;
using OhMyWord.Api.Responses.Game;
using OhMyWord.Api.Responses.Words;
using OhMyWord.Data.Models;
using OhMyWord.Data.Repositories;

namespace OhMyWord.Api.Services;

public interface IGameService
{
    Task<CurrentWordResponse> GetCurrentWord();
    Task<CurrentWordResponse> SelectNextWord();
    Task<GuessWordResponse> TestGuessAsync(GuessWordRequest request);
    Task<RegisterPlayerResponse> RegisterPlayerAsync(RegisterPlayerRequest request);
    Task UnregisterPlayerAsync(string connectionId);
}

public class GameService : IGameService
{
    private readonly ILogger<GameService> logger;
    private readonly IWordsRepository wordsRepository;
    private readonly IHubContext<GameHub, IGameHub> gameHubContext;
    private readonly IPlayerRepository playerRepository;

    private List<Word> allWords = new();
    private Word? currentWord;
    private DateTime currentWordExpiry;

    private GameServiceOptions Options { get; }

    public GameService(
        IOptions<GameServiceOptions> options,
        ILogger<GameService> logger,
        IWordsRepository wordsRepository,
        IHubContext<GameHub, IGameHub> gameHubContext,
        IPlayerRepository playerRepository)
    {
        this.logger = logger;
        this.wordsRepository = wordsRepository;
        this.gameHubContext = gameHubContext;
        this.playerRepository = playerRepository;
        Options = options.Value;
    }

    public async Task<CurrentWordResponse> GetCurrentWord()
    {
        var now = DateTime.UtcNow;

        // early exit if current word is still valid
        if (currentWord is not null && now < currentWordExpiry)
            return new CurrentWordResponse
            {
                Word = currentWord,
                Expiry = currentWordExpiry,
            };

        return await SelectNextWord();
    }

    public async Task<CurrentWordResponse> SelectNextWord()
    {
        // request new list of words from repository
        if (allWords.Count == 0) await RefreshWordsFromRepository();

        // set current word to randomly selected one
        var index = Random.Shared.Next(0, allWords.Count);
        currentWord = allWords[index];
        currentWordExpiry = DateTime.UtcNow.AddSeconds(Options.RoundLength);
        allWords.Remove(currentWord);

        logger.LogInformation("Current word selected as: {word}", currentWord);
        await gameHubContext.Clients.All.SendHint(new HintResponse(currentWord, DateTime.UtcNow.AddSeconds(Options.RoundLength)));

        return new CurrentWordResponse
        {
            Word = currentWord,
            Expiry = currentWordExpiry,
        };
    }

    public async Task<GuessWordResponse> TestGuessAsync(GuessWordRequest request)
    {
        var response = await GetCurrentWord();
        var correct = string.Equals(request.Value, response.Word.Value, StringComparison.InvariantCultureIgnoreCase);

        if (correct) await SelectNextWord();

        return new GuessWordResponse()
        {
            Value = request.Value.ToLowerInvariant(),
            Correct = correct,
        };
    }

    public async Task<RegisterPlayerResponse> RegisterPlayerAsync(RegisterPlayerRequest request)
    {
        var player = await playerRepository.FindPlayerByVisitorIdAsync(request.VisitorId);

        // create new player if existing player not found
        player ??= await playerRepository.CreatePlayerAsync(new Player
        {
            VisitorId = request.VisitorId,
            ConnectionId = request.ConnectionId
        });

        return new RegisterPlayerResponse()
        {
            Successful = true,
            PlayerId = player.Id
        };
    }

    public async Task UnregisterPlayerAsync(string connectionId)
    {
        var player = await playerRepository.FindPlayerByConnectionIdAsync(connectionId);

        if (player is null)
        {
            logger.LogWarning("Couldn't find a player with connection ID: {connectionId} to unregister.", connectionId);
            return;
        }

        await playerRepository.DeletePlayerAsync(player.Id);
    }

    private async Task RefreshWordsFromRepository()
    {
        allWords = new List<Word>(await wordsRepository.GetAllWordsAsync());
        logger.LogInformation("All words now contains {count} words.", allWords.Count);
    }
}