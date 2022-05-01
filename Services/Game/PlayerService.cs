using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Services.Data.Repositories;
using OhMyWord.Services.Events;
using System.Collections.Concurrent;

namespace OhMyWord.Services.Game;

public interface IPlayerService
{
    int PlayerCount { get; }

    /// <summary>
    /// Currently connected player IDs.
    /// </summary>
    IEnumerable<string> PlayerIds { get; }

    event EventHandler<PlayerEventArgs> PlayerAdded;
    event EventHandler<PlayerEventArgs> PlayerRemoved;

    Task<Player> AddPlayerAsync(string visitorId, string connectionId);
    Task RemovePlayerAsync(string connectionId);
    Task<bool> IncrementPlayerScoreAsync(string playerId, int points);
}

public class PlayerService : IPlayerService
{
    private readonly ILogger<PlayerService> logger;
    private readonly IPlayerRepository playerRepository;
    private readonly ConcurrentDictionary<string, Player> playerCache = new();

    public int PlayerCount => playerCache.Count;
    public IEnumerable<string> PlayerIds => playerCache.Keys;

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
            logger.LogDebug("Found existing player with visitorId: {visitorId}. Updating connection ID to: {connectionId}", visitorId, connectionId);
            await playerRepository.UpdatePlayerConnectionIdAsync(player.Id, connectionId);
        }

        // create new player if existing player not found
        player ??= await playerRepository.CreatePlayerAsync(new Player
        {
            VisitorId = visitorId,
            ConnectionId = connectionId
        });

        var wasAdded = playerCache.TryAdd(player.Id, player);
        if (!wasAdded)
        {
            logger.LogError("Could not add player with connection ID: {connectionId} to the local cache.", connectionId);
            // TODO: Deal with error here
        }

        PlayerAdded?.Invoke(this, new PlayerEventArgs(player.Id, PlayerCount));

        logger.LogInformation("Player with ID: {playerId} joined the game. Player count: {playerCount}", player.Id, PlayerCount);
        return player;
    }

    public async Task RemovePlayerAsync(string connectionId)
    {
        var player = playerCache.Values.FirstOrDefault(p => p.ConnectionId == connectionId);
        if (player is null)
        {
            logger.LogWarning("Couldn't find a player with connection ID: {connectionId} to remove.", connectionId);
            return;
        }

        var wasRemovedFromCache = playerCache.TryRemove(player.Id, out _);
        if (!wasRemovedFromCache)
            logger.LogError("Couldn't remove player with ID: {playerId} from cache.", player.Id);

        PlayerRemoved?.Invoke(this, new PlayerEventArgs(player.Id, PlayerCount));

        var wasUpdated = await playerRepository.UpdatePlayerConnectionIdAsync(player.Id, string.Empty);
        if (!wasUpdated)
            logger.LogWarning("Couldn't update player with ID: {playerId}.", player.Id);

        logger.LogInformation("Player with ID: {playerId} left the game. Player count: {playerCount}", player.Id, PlayerCount);
    }

    public Task<bool> IncrementPlayerScoreAsync(string playerId, int points) => playerRepository.IncrementPlayerScoreAsync(playerId, points); // TODO: Update local cache to keep points in sync
}
