using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OhMyWord.Core.Models;
using OhMyWord.Services.Data.Repositories;
using OhMyWord.Services.Models;
using OhMyWord.Services.Options;

namespace OhMyWord.Services.Game;

public interface IGameService
{
    Round? Round { get; }
    bool RoundActive { get; }
    int RoundNumber { get; }
    int PlayerCount { get; }
    DateTime Expiry { get; }

    event Action<RoundStart> RoundStarted;
    event Action<RoundEnd> RoundEnded;
    event Action<LetterHint> LetterHintAdded;

    Task ExecuteGameAsync(CancellationToken cancellationToken);
    Task<Player> AddPlayerAsync(string visitorId, string connectionId);
    Task RemovePlayerAsync(string connectionId);
    Task<int> ProcessGuessAsync(string playerId, string guess);
}

public class GameService : IGameService
{
    private readonly ILogger<GameService> logger;
    private readonly IWordsService wordsService;
    private readonly IPlayerRepository playerRepository;
    private readonly GameServiceOptions options;
    private readonly List<string> playerConnectionIds = new();

    public Round? Round { get; private set; }
    public bool RoundActive => Round is not null;
    public int RoundNumber { get; private set; }
    public int PlayerCount => playerConnectionIds.Count;
    public DateTime Expiry { get; private set; }

    public event Action<LetterHint>? LetterHintAdded;
    public event Action<RoundStart>? RoundStarted;
    public event Action<RoundEnd>? RoundEnded;

    public GameService(ILogger<GameService> logger, IWordsService wordsService, IPlayerRepository playerRepository, IOptions<GameServiceOptions> options)
    {
        this.logger = logger;
        this.wordsService = wordsService;
        this.playerRepository = playerRepository;
        this.options = options.Value;
    }

    public async Task ExecuteGameAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Round = await StartRoundAsync(++RoundNumber);
            await SendLetterHintsAsync(Round.Word, Round.WordHint, Round.Duration, cancellationToken);
            await EndRoundAsync(Round, cancellationToken);
        }
    }

    private async Task<Round> StartRoundAsync(int roundNumber)
    {
        var word = await wordsService.SelectRandomWordAsync();
        var duration = TimeSpan.FromSeconds(word.Value.Length * options.LetterHintDelay);
        var round = new Round(roundNumber, word, duration);
        Expiry = round.Expiry;
        RoundStarted?.Invoke(new RoundStart(round));
        logger.LogDebug("Round: {roundNumber} has started. Current currentWord is: {word}. Round duration: {seconds} seconds.",
            round.Number, round.Word, duration.Seconds);
        return round;
    }

    private async Task EndRoundAsync(Round round, CancellationToken cancellationToken)
    {
        var postRoundDelay = TimeSpan.FromSeconds(options.PostRoundDelay);
        Round = null;
        Expiry = DateTime.UtcNow + postRoundDelay;
        logger.LogDebug("Round: {number} has ended. Post round delay is: {seconds} seconds", round.Number, postRoundDelay.Seconds);
        RoundEnded?.Invoke(new RoundEnd(round, DateTime.UtcNow + postRoundDelay));
        await Task.Delay(postRoundDelay, cancellationToken);
    }

    private async Task SendLetterHintsAsync(Word word, WordHint wordHint, TimeSpan roundDelay, CancellationToken cancellationToken)
    {
        var letterDelay = roundDelay / word.Id.Length;
        var previousIndices = new List<int>();

        while (previousIndices.Count < word.Id.Length && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(letterDelay, cancellationToken);

            int index;
            do index = Random.Shared.Next(word.Id.Length);
            while (previousIndices.Contains(index));
            previousIndices.Add(index);

            var letterHint = new LetterHint
            {
                Position = index + 1,
                Value = word.Value[index]
            };

            logger.LogDebug("Added letter hint. Position: {position}, value: {value}", letterHint.Position, letterHint.Value);

            LetterHintAdded?.Invoke(letterHint);
            wordHint.AddLetterHint(letterHint);
        }
    }

    public async Task<int> ProcessGuessAsync(string playerId, string guess)
    {
        // if round is not active then immediately return false
        if (Round is null) return 0;

        // compare guess to current word value
        if (!string.Equals(guess, Round.Word.Value, StringComparison.InvariantCultureIgnoreCase))
            return 0;

        // look up player to award points
        var player = await playerRepository.FindPlayerByPlayerIdAsync(playerId);
        if (player is null)
        {
            logger.LogWarning("Couldn't find player with ID: {playerId} to award points to.", playerId);
            return 0;
        }

        const int points = 100;
        await playerRepository.IncrementPlayerScoreAsync(player.Id, points);

        return points;
    }

    public async Task<Player> AddPlayerAsync(string visitorId, string connectionId)
    {
        playerConnectionIds.Add(connectionId);

        var player = await playerRepository.FindPlayerByVisitorIdAsync(visitorId);
        if (player is not null) await playerRepository.UpdatePlayerConnectionIdAsync(player.Id, connectionId);

        // create new player if existing player not found
        player ??= await playerRepository.CreatePlayerAsync(new Player
        {
            VisitorId = visitorId,
            ConnectionId = connectionId
        });

        logger.LogInformation("Player with ID: {playerId} joined the game. Player count: {playerCount}", player.Id, PlayerCount);
        return player;
    }

    public async Task RemovePlayerAsync(string connectionId)
    {
        playerConnectionIds.Remove(connectionId);

        var player = await playerRepository.FindPlayerByConnectionIdAsync(connectionId);

        if (player is null)
        {
            logger.LogWarning("Couldn't find a player with connection ID: {connectionId} to remove. Player count: {playerCount}", connectionId, PlayerCount);
            return;
        }

        await playerRepository.UpdatePlayerConnectionIdAsync(player.Id, string.Empty);
        logger.LogInformation("Player with connection ID: {connectionId} left the game. Player count: {playerCount}", connectionId, PlayerCount);
    }
}
