using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Services;

namespace OhMyWord.Domain.Services;

public interface IUserService
{
    Task<User?> GetUserAsync(string userId, CancellationToken cancellationToken = default);
}

public class UserService : IUserService
{
    private readonly IUsersRepository usersRepository;

    public UserService(IUsersRepository usersRepository)
    {
        this.usersRepository = usersRepository;
    }

    public async Task<User?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var entity = await usersRepository.GetUserAsync(userId, cancellationToken);
        return entity?.ToUser();
    }
}
