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

    bool IsCorrect(string value);
    Task<Player> RegisterPlayerAsync(string visitorId, string connectionId);
    Task UnregisterPlayerAsync(string connectionId);
}

public class GameService : IGameService
{
    private readonly ILogger<GameService> logger;
    private readonly IWordsService wordsService;
    private readonly IPlayerRepository playerRepository;
    private readonly GameServiceOptions options;

    public Round? Round { get; private set; }
    public bool RoundActive => Round is not null;
    public int RoundNumber { get; private set; }
    public int PlayerCount { get; private set; }
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

    public bool IsCorrect(string value) => string.Equals(value, Round?.Word.Value, StringComparison.InvariantCultureIgnoreCase);

    public async Task<Player> RegisterPlayerAsync(string visitorId, string connectionId)
    {
        var player = await playerRepository.FindPlayerByVisitorIdAsync(visitorId);

        // create new player if existing player not found
        if (player is null)
        {
            var result = await playerRepository.CreatePlayerAsync(new Player
            {
                VisitorId = visitorId,
                ConnectionId = connectionId
            });

            player = result.Resource ?? throw new NullReferenceException("Player resource is null!");
        }

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
