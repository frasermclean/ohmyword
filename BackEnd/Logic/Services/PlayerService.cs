using FluentResults;
using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using OhMyWord.Logic.Contracts.Parameters;

namespace OhMyWord.Logic.Services;

public interface IPlayerService
{
    Task<Player> GetPlayerAsync(GetPlayerParameters parameters,
        CancellationToken cancellationToken = default);

    Task IncrementPlayerScoreAsync(Guid playerId, int points);
}

public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository playerRepository;
    private readonly IGraphApiClient graphApiClient;
    private readonly IGeoLocationService geoLocationService;

    private static int guestCount;

    public PlayerService(IPlayerRepository playerRepository, IGraphApiClient graphApiClient,
        IGeoLocationService geoLocationService)
    {
        this.playerRepository = playerRepository;
        this.graphApiClient = graphApiClient;
        this.geoLocationService = geoLocationService;
    }

    public async Task<Player> GetPlayerAsync(GetPlayerParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var playerTask = GetOrCreatePlayerAsync(parameters, cancellationToken);
        var nameTask = GetPlayerNameAsync(parameters.UserId, cancellationToken);
        var geoLocationTask = geoLocationService.GetGeoLocationAsync(parameters.IpAddress, cancellationToken);

        await Task.WhenAll(playerTask, nameTask, geoLocationTask);

        return playerTask.Result.Value with
        {
            Name = nameTask.Result,
            ConnectionId = parameters.ConnectionId,
            VisitorId = parameters.VisitorId,
            UserId = parameters.UserId,
            IpAddress = parameters.IpAddress,
            GeoLocation = geoLocationTask.Result.Value
        };
    }

    public Task IncrementPlayerScoreAsync(Guid playerId, int points) =>
        playerRepository.IncrementScoreAsync(playerId, points);

    private async Task<Result<Player>> GetOrCreatePlayerAsync(GetPlayerParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var result = await playerRepository.GetPlayerAsync(parameters.PlayerId, cancellationToken);
        if (result.IsSuccess)
        {
            return await playerRepository.UpdatePlayerAsync(result.Value.Id, parameters.VisitorId,
                parameters.IpAddress.ToString(), cancellationToken);
        }

        var player = await CreatePlayerFromParameters(parameters, cancellationToken);
        return await playerRepository.CreatePlayerAsync(player, cancellationToken);
    }

    private async Task<Player> CreatePlayerFromParameters(GetPlayerParameters parameters,
        CancellationToken cancellationToken) => new()
    {
        Id = parameters.PlayerId,
        Name = await GetPlayerNameAsync(parameters.UserId, cancellationToken),
        ConnectionId = parameters.ConnectionId,
        VisitorId = parameters.VisitorId,
        UserId = parameters.UserId,
        Score = default,
        RegistrationCount = 1,
        IpAddress = parameters.IpAddress
    };

    private async Task<string> GetPlayerNameAsync(Guid? userId, CancellationToken cancellationToken = default)
    {
        if (!userId.HasValue)
            return $"Guest {++guestCount}";

        var (_, _, displayName) = await graphApiClient.GetUserDetailsAsync(userId.Value, cancellationToken);

        return displayName ?? "Unknown";
    }
}
