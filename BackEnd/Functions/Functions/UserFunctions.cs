using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OhMyWord.Functions.Models;
using OhMyWord.Infrastructure.Entities;
using OhMyWord.Infrastructure.Services;

namespace OhMyWord.Functions.Functions;

public sealed class UserFunctions
{
    private readonly ILogger<UserFunctions> logger;
    private readonly IUsersRepository usersRepository;

    public UserFunctions(ILogger<UserFunctions> logger, IUsersRepository usersRepository)
    {
        this.logger = logger;
        this.usersRepository = usersRepository;
    }

    [Function(nameof(GetUserClaims))]
    public async Task<HttpResponseData> GetUserClaims(
        [HttpTrigger("post", Route = "get-user-claims")]
        HttpRequestData httpRequest
    )
    {
        var getUserClaimsRequest = await httpRequest.ReadFromJsonAsync<GetUserClaimsRequest>();
        if (getUserClaimsRequest is null) throw new InvalidOperationException("Couldn't deserialize request body");

        var user = await usersRepository.GetUserAsync(getUserClaimsRequest.UserId);

        // create user if it doesn't exist
        if (user is null)
            await usersRepository.CreateUserAsync(new UserEntity
            {
                Id = getUserClaimsRequest.UserId,
                Name = getUserClaimsRequest.Name,
                Email = getUserClaimsRequest.Email,
                Role = Role.User
            });

        var role = user?.Role ?? Role.User;

        logger.LogInformation("Authenticated user ID is: {UserId}, determined role as: {Role}",
            getUserClaimsRequest.UserId, role);

        var response = httpRequest.CreateResponse();
        await response.WriteAsJsonAsync(new GetUserClaimsResponse { Role = role });
        return response;
    }
}
