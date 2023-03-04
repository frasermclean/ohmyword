using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OhMyWord.Data.Entities;
using OhMyWord.Data.Services;
using OhMyWord.Functions.Models;
using System.Net;

namespace OhMyWord.Functions;

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
                Name = getUserClaimsRequest.Name, Email = getUserClaimsRequest.Email
            });

        var role = user?.Role ?? Role.User;

        logger.LogInformation("Authenticated user ID is: {UserId}, determined role as: {Role}",
            getUserClaimsRequest.UserId, role);

        var response = httpRequest.CreateResponse();
        await response.WriteAsJsonAsync(new GetUserClaimsResponse { Role = role });
        return response;
    }
}
