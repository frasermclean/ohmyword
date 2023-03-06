﻿using OhMyWord.Domain.Extensions;
using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Services;

namespace OhMyWord.Domain.Services;

public interface IUsersService
{
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