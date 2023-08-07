using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Domain.Services;
using OhMyWord.Functions.Models;

namespace OhMyWord.Functions.Functions;

public sealed class UserFunctions
{
    private readonly ILogger<UserFunctions> logger;
    private readonly IUsersService usersService;

    public UserFunctions(ILogger<UserFunctions> logger, IUsersService usersService)
    {
        this.logger = logger;
        this.usersService = usersService;
    }

    [Function(nameof(GetUserClaims))]
    public async Task<HttpResponseData> GetUserClaims(
        [HttpTrigger("post", Route = "get-user-claims")]
        HttpRequestData httpRequest
    )
    {
        var getUserClaimsRequest = await httpRequest.ReadFromJsonAsync<GetUserClaimsRequest>();
        if (getUserClaimsRequest is null) throw new InvalidOperationException("Couldn't deserialize request body");

        var user = await usersService.GetUserAsync(getUserClaimsRequest.UserId);

        // create user if it doesn't exist
        user ??= await usersService.CreateUserAsync(new User
        {
            Id = getUserClaimsRequest.UserId,
            Name = getUserClaimsRequest.Name,
            Email = getUserClaimsRequest.Email,
            IdentityProvider = getUserClaimsRequest.IdentityProvider,
            Role = Role.User
        });

        var role = user.Role;

        logger.LogInformation("Authenticated user ID is: {UserId}, determined role as: {Role}",
            getUserClaimsRequest.UserId, role);

        var response = httpRequest.CreateResponse();
        await response.WriteAsJsonAsync(new GetUserClaimsResponse { Role = role });
        return response;
    }
}
