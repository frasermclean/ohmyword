using OhMyWord.Core.Models;
using OhMyWord.Domain.Extensions;
using OhMyWord.Infrastructure.Services.Repositories;

namespace OhMyWord.Domain.Services;

public interface IUsersService
{
    IAsyncEnumerable<User> SearchUsers(string? filter = default, CancellationToken cancellationToken = default);
    Task<User?> GetUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<User> CreateUserAsync(User user);
}

public class UsersService : IUsersService
{
    private readonly IUsersRepository usersRepository;

    public UsersService(IUsersRepository usersRepository)
    {
        this.usersRepository = usersRepository;
    }

    public IAsyncEnumerable<User> SearchUsers(string? filter, CancellationToken cancellationToken)
    {
        return usersRepository.SearchUsers(filter, cancellationToken)
            .Select(entity => entity.ToUser());
    }

    public async Task<User?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var entity = await usersRepository.GetUserAsync(userId, cancellationToken);
        return entity?.ToUser();
    }

    public async Task<User> CreateUserAsync(User user)
    {
        await usersRepository.UpsertUserAsync(user.ToEntity());
        return user;
    }
}
