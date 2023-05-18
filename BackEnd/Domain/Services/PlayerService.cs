using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Services;
using System.Collections.Concurrent;
using System.Net;

namespace OhMyWord.Domain.Services;

public interface IPlayerService
{
    int PlayerCount { get; }

    /// <summary>
    /// Currently connected player IDs.
    /// </summary>
    IEnumerable<string> PlayerIds { get; }

    Task<Player> AddPlayerAsync(string visitorId, string connectionId, IPAddress ipAddress, Guid? userId = default);
    Player? RemovePlayer(string connectionId);
    Player? GetPlayerByConnectionId(string connectionId);

    /// <summary>
    /// Get a <see cref="Player"/> by it's ID.
    /// </summary>
    /// <param name="playerId">The ID of the player to look up.</param>
    /// <returns>The <see cref="Player"/> object if found, null if not.</returns>
    Player? GetPlayerById(string playerId);

    Task IncrementPlayerScoreAsync(string visitorId, int points);
}

public class PlayerService : IPlayerService
{
    private readonly ILogger<PlayerService> logger;
    private readonly IPlayerRepository playerRepository;

    /// <summary>
    /// Local cache of players with connection ID as key.
    /// </summary>
    private readonly ConcurrentDictionary<string, Player> players = new();

    public int PlayerCount => players.Count;
    public IEnumerable<string> PlayerIds => players.Values.Select(player => player.Id);

    public PlayerService(ILogger<PlayerService> logger, IPlayerRepository playerRepository)
    {
        this.logger = logger;
        this.playerRepository = playerRepository;
    }

    public async Task<Player> AddPlayerAsync(string visitorId, string connectionId, IPAddress ipAddress, Guid? userId)
    {
        var player = (await playerRepository.GetPlayerAsync(visitorId))?.ToPlayer(connectionId);
        if (player is not null)
        {
            logger.LogDebug("Found existing visitor with ID: {VisitorId}", visitorId);

            // TODO: Handle multiple connections with same visitor ID
            await playerRepository.IncrementRegistrationCountAsync(player.Id);
            if (!player.IpAddresses.Contains(ipAddress))
                await playerRepository.AddIpAddressAsync(player.Id, ipAddress.ToString());
        }

        // create player if existing player was not found
        player ??= (await playerRepository.CreatePlayerAsync(new PlayerEntity
            {
                Id = visitorId, UserId = userId, IpAddresses = new[] { ipAddress.ToString() }
            }))
            .ToPlayer(connectionId);

        players[connectionId] = player;
        logger.LogInformation("Player with ID: {PlayerId} joined the game", player.Id);

        return player;
    }

    public Player? RemovePlayer(string connectionId)
    {
        if (players.TryRemove(connectionId, out var player))
        {
            logger.LogInformation("Player with ID: {PlayerID} left the game", player.Id);
            return player;
        }

        logger.LogError("Couldn't remove player with connection ID: {ConnectionId} from cache", connectionId);
        return default;
    }

    public Player? GetPlayerById(string playerId)
        => players.FirstOrDefault(pair => pair.Value.Id == playerId).Value;

    public Player? GetPlayerByConnectionId(string connectionId)
        => players.TryGetValue(connectionId, out var player) ? player : default;

    public Task IncrementPlayerScoreAsync(string visitorId, int points) =>
        playerRepository.IncrementScoreAsync(visitorId, points); // TODO: Update local cache to keep points in sync
}
