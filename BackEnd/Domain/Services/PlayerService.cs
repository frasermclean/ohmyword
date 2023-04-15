using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Entities;
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
    void RemovePlayer(string connectionId);
    Player GetPlayer(string connectionId);
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

        var wasAdded = players.TryAdd(connectionId, player);
        if (!wasAdded)
            logger.LogWarning("Player with connection ID: {ConnectionId} already exists in the local cache",
                connectionId);

        logger.LogInformation("Player with ID: {VisitorId} joined the game. Player count: {PlayerCount}", player.Id,
            PlayerCount);

        return player;
    }

    public void RemovePlayer(string connectionId)
    {
        if (players.TryRemove(connectionId, out var visitor))
        {
            logger.LogInformation("Player with ID: {VisitorId} left the game. Player count: {PlayerCount}",
                visitor.Id, PlayerCount);
        }
        else
        {
            logger.LogError("Couldn't remove visitor with connection ID: {ConnectionId} from cache", connectionId);
        }
    }

    public Player GetPlayer(string connectionId) => players[connectionId];

    public Task IncrementPlayerScoreAsync(string visitorId, int points) =>
        playerRepository.IncrementScoreAsync(visitorId, points); // TODO: Update local cache to keep points in sync
}
