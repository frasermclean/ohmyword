using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;
using OhMyWord.Infrastructure.Services;
using System.Net;

namespace OhMyWord.Domain.Services;

public interface IPlayerService
{
    Task<Player> GetOrCreatePlayerEntityAsync(Guid playerId, string visitorId, string connectionId,
        IPAddress ipAddress, Guid? userId = default);

    Task PatchPlayerRegistrationAsync(Player player, string visitorId, IPAddress ipAddress);

    Task IncrementPlayerScoreAsync(Guid playerId, int points);
}

public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository playerRepository;

    public PlayerService(IPlayerRepository playerRepository)
    {
        this.playerRepository = playerRepository;
    }

    public async Task<Player> GetOrCreatePlayerEntityAsync(Guid playerId, string visitorId, string connectionId,
        IPAddress ipAddress, Guid? userId = default)
    {
        var entity = await playerRepository.GetPlayerByIdAsync(playerId) ??
                     await playerRepository.CreatePlayerAsync(new PlayerEntity
                     {
                         Id = playerId.ToString(),
                         UserId = userId,
                         VisitorIds = new[] { visitorId },
                         IpAddresses = new[] { ipAddress.ToString() }
                     });

        return entity.ToPlayer(connectionId, visitorId, ipAddress);
    }

    public async Task PatchPlayerRegistrationAsync(Player player, string visitorId, IPAddress ipAddress)
    {
        await playerRepository.IncrementRegistrationCountAsync(player.Id);

        // // patch ip address
        // if (!player.IpAddresses.Contains(ipAddress))
        //     await playerRepository.AddIpAddressAsync(player.Id, ipAddress.ToString());
        //
        // // patch visitor id
        // if (!player.VisitorIds.Contains(visitorId))
        //     await playerRepository.AddVisitorIdAsync(player.Id, visitorId);
    }

    public Task IncrementPlayerScoreAsync(Guid playerId, int points) =>
        playerRepository.IncrementScoreAsync(playerId, points);
}
