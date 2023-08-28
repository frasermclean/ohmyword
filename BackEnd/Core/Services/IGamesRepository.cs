using FluentResults;
using OhMyWord.Core.Models;

namespace OhMyWord.Core.Services;

public interface IGamesRepository
{
    /// <summary>
    /// Create a new game session.
    /// </summary>
    /// <param name="session">The session to create</param>
    /// <param name="cancellationToken">Task cancellation token</param>
    /// <returns>A <see cref="Result"/> with <see cref="Session"/> value</returns>
    Task<Result<Session>> CreateSessionAsync(Session session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new game round.
    /// </summary>
    /// <param name="round">The round to create</param>
    /// <param name="cancellationToken">Task cancellation token</param>
    /// <returns>A <see cref="Result"/> with <see cref="Round"/> value</returns>
    Task<Result<Round>> CreateRoundAsync(Round round, CancellationToken cancellationToken = default);
}
