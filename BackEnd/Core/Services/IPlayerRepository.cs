using FluentResults;
using OhMyWord.Core.Models;

namespace OhMyWord.Core.Services;

public interface IPlayerRepository
{
    Task<Result<Player>> GetPlayerAsync(Guid playerId, CancellationToken cancellationToken = default);
    Task<Result<Player>> CreatePlayerAsync(Player player, CancellationToken cancellationToken = default);
    Task<Result> DeletePlayerAsync(Guid playerId, CancellationToken cancellationToken = default);

    Task<Result<Player>> UpdatePlayerAsync(Guid playerId, string visitorId, string ipAddress,
        CancellationToken cancellationToken);

    Task<Result<Player>> IncrementScoreAsync(Guid playerId, long value);
}
