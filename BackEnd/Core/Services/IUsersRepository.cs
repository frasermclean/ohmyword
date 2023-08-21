using OhMyWord.Core.Models;

namespace OhMyWord.Core.Services;

public interface IUsersRepository
{
    IAsyncEnumerable<User> SearchUsers(string? filter = default, CancellationToken cancellationToken = default);
    Task<User?> GetUserAsync(string userId, CancellationToken cancellationToken = default);
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
}
