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

    [Function(nameof(ProcessUserClaims))]
    public async Task<HttpResponseData> ProcessUserClaims(
        [HttpTrigger("post", Route = "process-user-claims")]
        HttpRequestData httpRequest,
        [FromBody] ProcessUserClaimsRequest request
    )
    {
        logger.LogInformation("Processing request: {Request}", request);

        var user = await usersService.GetUserAsync(request.UserId);

        // create user if it doesn't exist
        user ??= await usersService.CreateUserAsync(new User
        {
            Id = request.UserId,
            Name = request.Name,
            Email = request.Email,
            IdentityProvider = request.IdentityProvider,
            Role = Role.User
        });

        var role = user.Role;

        logger.LogInformation("Authenticated user ID is: {UserId}, determined role as: {Role}",
            request.UserId, role);

        var response = httpRequest.CreateResponse();
        await response.WriteAsJsonAsync(new ProcessUserClaimsResponse { Role = role });
        return response;
    }
}
