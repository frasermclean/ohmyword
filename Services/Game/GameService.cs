using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Core.Models;
using OhMyWord.Services.Data.Repositories;
using OhMyWord.Services.Options;

namespace OhMyWord.Services.Game;

public interface IGameService
{
    int PlayerCount { get; }
    bool RoundActive { get; }
    int RoundNumber { get; }

    Task<TimeSpan> StartRoundAsync();
    Task<TimeSpan> EndRoundAsync();
    bool IsCorrect(string value);
    WordHint GetHint();
    GameStatus GetGameStatus();
    Task<Player> RegisterPlayerAsync(string visitorId, string connectionId);
    Task UnregisterPlayerAsync(string connectionId);
}

public class GameService : IGameService
{
    private readonly ILogger<GameService> logger;
    private readonly IWordsRepository wordsRepository;
    private readonly IPlayerRepository playerRepository;
    private readonly List<Word> words = new();

    private Word word = Word.Default;
    private DateTime expiry;

    private GameServiceOptions Options { get; }

    public int PlayerCount { get; private set; }
    public int RoundNumber { get; private set; }
    public bool RoundActive { get; private set; }

    public GameService(
        ILogger<GameService> logger,
        IWordsRepository wordsRepository,
        IPlayerRepository playerRepository,
        IOptions<GameServiceOptions> options)
    {
        this.logger = logger;
        this.wordsRepository = wordsRepository;
        this.playerRepository = playerRepository;

        Options = options.Value;
    }

    public async Task<TimeSpan> StartRoundAsync()
    {
        // request new list of words from repository
        if (words.Count == 0) words.AddRange(await wordsRepository.GetAllWordsAsync());

        // ensure words were actually retrieved
        if (words.Count > 0)
        {
            // set current word to randomly selected one
            var index = Random.Shared.Next(0, words.Count);
            word = words[index];
            expiry = DateTime.UtcNow.AddSeconds(Options.RoundLength);

            // remove word from list of words
            var wasRemoved = words.Remove(word);
            if (!wasRemoved) logger.LogWarning("Word: {word} at index: {index} couldn't be removed from the list.", word, index);
        }
        else
        {
            logger.LogError("No words were retrieved from the database!");
            word = Word.Default;
        }

        RoundActive = true;
        logger.LogInformation("Round number: {RoundNumber} active, current word: {word}", ++RoundNumber, word);
        return expiry - DateTime.UtcNow;
    }

    public async Task<TimeSpan> EndRoundAsync()
    {
        expiry = DateTime.UtcNow.AddSeconds(Options.PostRoundDelay);
        RoundActive = false;
        return expiry - DateTime.UtcNow;
    }

    public bool IsCorrect(string value) => string.Equals(value, word.Id, StringComparison.InvariantCultureIgnoreCase);

    public WordHint GetHint() => new(word);

    public GameStatus GetGameStatus() => new()
    {
        Expiry = expiry,
        PlayerCount = PlayerCount,
        RoundActive = RoundActive,
    };

    public async Task<Player> RegisterPlayerAsync(string visitorId, string connectionId)
    {
        var player = await playerRepository.FindPlayerByVisitorIdAsync(visitorId);

        // create new player if existing player not found
        player ??= await playerRepository.CreatePlayerAsync(new Player
        {
            VisitorId = visitorId,
            ConnectionId = connectionId
        });

        logger.LogInformation("Player with ID: {playerId} joined the game. Player count: {playerCount}", player.Id, ++PlayerCount);

        return player;
    }

    public async Task UnregisterPlayerAsync(string connectionId)
    {
        --PlayerCount;

        var player = await playerRepository.FindPlayerByConnectionIdAsync(connectionId);

        if (player is null)
        {
            logger.LogWarning("Couldn't find a player with connection ID: {connectionId} to unregister. Player count: {playerCount}", connectionId, PlayerCount);
            return;
        }

        await playerRepository.DeletePlayerAsync(player);
        logger.LogInformation("Player with connection ID: {connectionId} left the game. Player count: {playerCount}", connectionId, PlayerCount);
    }
}
