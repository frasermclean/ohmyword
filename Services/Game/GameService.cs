using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Core.Models;
using OhMyWord.Services.Data.Repositories;
using OhMyWord.Services.Options;

namespace OhMyWord.Services.Game;

public interface IGameService
{
    GameStatus GameStatus { get; }
    WordHint WordHint { get; }
    int PlayerCount { get; }

    event Action<GameStatus> GameStatusChanged;
    event Action<WordHint> WordHintChanged;

    Task StartGameAsync(CancellationToken cancellationToken);

    bool IsCorrect(string value);
    Task<Player> RegisterPlayerAsync(string visitorId, string connectionId);
    Task UnregisterPlayerAsync(string connectionId);
}

public class GameService : IGameService
{
    private readonly ILogger<GameService> logger;
    private readonly IWordsRepository wordsRepository;
    private readonly IPlayerRepository playerRepository;

    private WordHint wordHint = WordHint.Default;
    private GameStatus gameStatus = new();

    private GameServiceOptions Options { get; }

    public GameStatus GameStatus
    {
        get => gameStatus;
        private set
        {
            gameStatus = value;
            GameStatusChanged?.Invoke(value);
        }
    }

    public int PlayerCount { get; private set; }

    private Word Word { get; set; } = Word.Default;

    public WordHint WordHint
    {
        get => wordHint;
        private set
        {
            wordHint = value;
            WordHintChanged?.Invoke(value);
        }
    }

    public event Action<GameStatus>? GameStatusChanged;
    public event Action<WordHint>? WordHintChanged;

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

    public async Task StartGameAsync(CancellationToken cancellationToken)
    {
        var words = await LoadWordsFromDatabaseAsync();
        var previousIndices = new List<int>();

        while (!cancellationToken.IsCancellationRequested)
        {
            Word = SelectNextRandomWord(words, previousIndices);
            WordHint = new WordHint(Word);

            var delay = TimeSpan.FromSeconds(Options.RoundLength);
            GameStatus = new GameStatus
            {
                RoundActive = true,
                RoundNumber = GameStatus.RoundNumber + 1,
                Expiry = DateTime.UtcNow + delay
            };
            logger.LogDebug("Round: {RoundNumber} has started. Current word is: {Word}", gameStatus.RoundNumber, Word);
            await Task.Delay(delay, cancellationToken);

            delay = TimeSpan.FromSeconds(Options.PostRoundDelay);
            GameStatus = GameStatus with
            {
                RoundActive = false,
                Expiry = DateTime.UtcNow + delay
            };

            logger.LogDebug("Round: {RoundNumber} has ended.", gameStatus.RoundNumber);
            await Task.Delay(delay, cancellationToken);
        }
    }

    private async Task<List<Word>> LoadWordsFromDatabaseAsync()
    {
        List<Word> words = new(await wordsRepository.GetAllWordsAsync());

        if (words.Count == 0)
        {
            logger.LogWarning("No words were retrieved from the database!");
            words.Add(Word.Default);
        }
        else
            logger.LogInformation("Retrieved: {count} words from database.", words.Count);

        return words;
    }

    private static Word SelectNextRandomWord(IReadOnlyList<Word> words, ICollection<int> previousIndices)
    {
        if (previousIndices.Count == words.Count)
            previousIndices.Clear();

        int index;
        do index = Random.Shared.Next(0, words.Count);
        while (previousIndices.Contains(index));

        var randomWord = words[index];
        previousIndices.Add(index);

        return randomWord;
    }

    public bool IsCorrect(string value) => string.Equals(value, Word.Id, StringComparison.InvariantCultureIgnoreCase);

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
