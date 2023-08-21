using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using OhMyWord.Functions.Models;

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

    [Function(nameof(ProcessUserClaims))]
    public async Task<HttpResponseData> ProcessUserClaims(
        [HttpTrigger("post", Route = "process-user-claims")]
        HttpRequestData httpRequest,
        [FromBody] ProcessUserClaimsRequest request,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation("Processing request: {Request}", request);

        var user = await usersRepository.GetUserAsync(request.UserId, cancellationToken) ??
                   await CreateUserAsync(request, cancellationToken);

        var role = user.Role;

        logger.LogInformation("Authenticated user ID is: {UserId}, determined role as: {Role}",
            request.UserId, role);

        var response = httpRequest.CreateResponse();
        await response.WriteAsJsonAsync(new ProcessUserClaimsResponse { Role = role },
            cancellationToken: cancellationToken);
        return response;
    }

    private async Task<User> CreateUserAsync(ProcessUserClaimsRequest request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = request.UserId,
            Name = request.Name,
            Email = request.Email,
            IdentityProvider = request.IdentityProvider,
            Role = Role.User
        };

        await usersRepository.AddUserAsync(user, cancellationToken);
        return user;
    }
}
