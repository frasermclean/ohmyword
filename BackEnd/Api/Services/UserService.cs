using OhMyWord.Api.Extensions;
using OhMyWord.Api.Models;
using OhMyWord.Data.Services;

namespace OhMyWord.Api.Services;

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
