using FluentResults;
using OhMyWord.Core.Models;

namespace OhMyWord.Core.Services;

public interface IUsersRepository
{
    IAsyncEnumerable<User> SearchUsers(string? filter = default, CancellationToken cancellationToken = default);
    Task<Result<User>> GetUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> CreateUserAsync(User user, CancellationToken cancellationToken = default);
}
