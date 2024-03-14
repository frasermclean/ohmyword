using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using System.Collections.Concurrent;

namespace OhMyWord.Domain.Services.State;

public interface IPlayerState : IState
{
    /// <summary>
    /// Number of currently connected players.
    /// </summary>
    int PlayerCount { get; }

    /// <summary>
    /// Currently connected player IDs.
    /// </summary>
    IEnumerable<Guid> PlayerIds { get; }

    /// <summary>
    /// Add a player to the state.
    /// </summary>
    /// <param name="player">The <see cref="Player"/> to add.</param>
    /// <returns>True on success, false on failure.</returns>
    bool AddPlayer(Player player);

    /// <summary>
    /// Remove a player from the state.
    /// </summary>
    /// <param name="connectionId">The connection ID of the player to remove.</param>
    /// <returns>True on success, false on failure.</returns>
    bool RemovePlayer(string connectionId);

    /// <summary>
    /// Attempt to get a <see cref="Player"/> by it's connection ID.
    /// </summary>
    /// <param name="connectionId">The connection ID to search for.</param>
    /// <returns>A <see cref="Player"/> reference if found, null if not.</returns>
    Player? GetPlayerByConnectionId(string connectionId);

    /// <summary>
    /// Get a <see cref="Player"/> by it's ID.
    /// </summary>
    /// <param name="playerId">The ID of the player to look up.</param>
    /// <returns>A <see cref="Player"/> reference if found, null if not.</returns>
    Player? GetPlayerById(Guid playerId);
}

public class PlayerState : IPlayerState
{
    private readonly ILogger<PlayerState> logger;

    /// <summary>
    /// Local cache of players with connection ID as key.
    /// </summary>
    private readonly ConcurrentDictionary<string, Player> players = new();

    public PlayerState(ILogger<PlayerState> logger)
    {
        this.logger = logger;
    }

    public int PlayerCount => players.Count;
    public IEnumerable<Guid> PlayerIds => players.Values.Select(player => player.Id);
    public bool IsDefault => players.IsEmpty;

    public bool AddPlayer(Player player)
    {
        if (!players.TryAdd(player.ConnectionId, player))
        {
            logger.LogError("Couldn't add player with connection ID: {ConnectionId}", player.ConnectionId);
            return false;
        }

        logger.LogInformation("Added player with connection ID: {ConnectionId}", player.ConnectionId);
        return true;
    }

    public bool RemovePlayer(string connectionId)
    {
        if (players.TryRemove(connectionId, out _))
        {
            logger.LogInformation("Removed player with connection ID: {PlayerID}", connectionId);
            return true;
        }

        logger.LogError("Couldn't remove player with connection ID: {ConnectionId}", connectionId);
        return false;
    }

    public Player? GetPlayerById(Guid playerId)
        => players.FirstOrDefault(pair => pair.Value.Id == playerId).Value;

    public Player? GetPlayerByConnectionId(string connectionId)
        => players.TryGetValue(connectionId, out var player) ? player : default;


    public void Reset() => players.Clear();
}
