using Microsoft.Extensions.Logging;
using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Services;
using System.Net;

namespace OhMyWord.Domain.Services;

public interface IPlayerService
{
    Task<Player> GetOrCreatePlayerAsync(string visitorId, string connectionId, IPAddress ipAddress,
        Guid? userId = default);

    Task IncrementPlayerScoreAsync(string visitorId, int points);
}

public class PlayerService : IPlayerService
{
    private readonly ILogger<PlayerService> logger;
    private readonly IPlayerRepository playerRepository;

    public PlayerService(ILogger<PlayerService> logger, IPlayerRepository playerRepository)
    {
        this.logger = logger;
        this.playerRepository = playerRepository;
    }

    public async Task<Player> GetOrCreatePlayerAsync(string visitorId, string connectionId, IPAddress ipAddress,
        Guid? userId)
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


        logger.LogInformation("Player with ID: {PlayerId} joined the game", player.Id);

        return player;
    }


    public Task IncrementPlayerScoreAsync(string visitorId, int points) =>
        playerRepository.IncrementScoreAsync(visitorId, points); // TODO: Update local cache to keep points in sync
}
