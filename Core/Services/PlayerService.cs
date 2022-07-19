using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using OhMyWord.Core.Events;
using OhMyWord.Data.Models;
using OhMyWord.Data.Services;

namespace OhMyWord.Core.Services;

public interface IPlayerService
{
    int PlayerCount { get; }

    /// <summary>
    /// Currently connected player IDs.
    /// </summary>
    IEnumerable<Guid> PlayerIds { get; }

    event EventHandler<PlayerEventArgs> PlayerAdded;
    event EventHandler<PlayerEventArgs> PlayerRemoved;

    Task<Player> AddPlayerAsync(string visitorId, string connectionId);
    void RemovePlayer(string connectionId);
    Player GetPlayer(string connectionId);
    Task<bool> IncrementPlayerScoreAsync(Guid playerId, int points);
}

public class PlayerService : IPlayerService
{
    private readonly ILogger<PlayerService> logger;
    private readonly IPlayerRepository playerRepository;
    private readonly ConcurrentDictionary<string, Player> playerCache = new();

    public int PlayerCount => playerCache.Count;
    public IEnumerable<Guid> PlayerIds => playerCache.Values.Select(player => player.Id);

    public event EventHandler<PlayerEventArgs>? PlayerAdded;
    public event EventHandler<PlayerEventArgs>? PlayerRemoved;

    public PlayerService(ILogger<PlayerService> logger, IPlayerRepository playerRepository)
    {
        this.logger = logger;
        this.playerRepository = playerRepository;
    }

    public async Task<Player> AddPlayerAsync(string visitorId, string connectionId)
    {
        var player = await playerRepository.FindPlayerByVisitorIdAsync(visitorId);
        if (player is not null)
        {
            // TODO: Handle multiple connections with same visitor ID
            await playerRepository.IncrementPlayerRegistrationCountAsync(player.Id);
            logger.LogDebug("Found existing player with visitorId: {visitorId}.", visitorId);
        }

        // create new player if existing player not found
        player ??= await playerRepository.CreatePlayerAsync(new Player
        {
            VisitorId = visitorId,
        });

        var wasAdded = playerCache.TryAdd(connectionId, player);
        if (!wasAdded)
            logger.LogWarning("Player with connection ID: {connectionId} already exists in the local cache.",
                connectionId);

        PlayerAdded?.Invoke(this, new PlayerEventArgs(player.Id, PlayerCount, connectionId));

        logger.LogInformation("Player with ID: {playerId} joined the game. Player count: {playerCount}", player.Id,
            PlayerCount);
        return player;
    }

    public void RemovePlayer(string connectionId)
    {
        if (playerCache.TryRemove(connectionId, out var player))
        {
            PlayerRemoved?.Invoke(this, new PlayerEventArgs(player.Id, PlayerCount, connectionId));
            logger.LogInformation("Player with ID: {playerId} left the game. Player count: {playerCount}", player.Id,
                PlayerCount);
        }
        else
        {
            logger.LogError("Couldn't remove player with connection ID: {connectionId} from cache.", connectionId);
        }
    }

    public Player GetPlayer(string connectionId) => playerCache[connectionId];

    public Task<bool> IncrementPlayerScoreAsync(Guid playerId, int points) =>
        playerRepository.IncrementPlayerScoreAsync(playerId, points); // TODO: Update local cache to keep points in sync
}