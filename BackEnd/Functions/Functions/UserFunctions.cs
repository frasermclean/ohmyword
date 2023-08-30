using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OhMyWord.Core.Models;
using OhMyWord.Core.Services;
using OhMyWord.Functions.Models;
using System.Net;

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
        [FromBody] ProcessUserClaimsRequest request
    )
    {
        logger.LogInformation("Processing request: {Request}", request);

        var result = await usersRepository.GetUserAsync(request.UserId);
        if (result.IsSuccess)
        {
            logger.LogInformation("Found existing user with ID: {UserId}, determined role as: {Role}",
                request.UserId, result.Value.Role);
            return await CreateSuccessResponseAsync(result.Value.Role);
        }

        // create user if it doesn't exist
        var createResult = await usersRepository.CreateUserAsync(new User
        {
            Id = request.UserId,
            Name = request.Name,
            Email = request.Email,
            IdentityProvider = request.IdentityProvider,
            Role = Role.User
        });

        if (createResult.IsFailed)
            return await CreateErrorResponseAsync(createResult.Errors.FirstOrDefault()?.Message ?? string.Empty);

        logger.LogInformation("Created new user with ID: {UserId}", request.UserId);

        return await CreateSuccessResponseAsync(Role.User);

        async Task<HttpResponseData> CreateSuccessResponseAsync(Role role)
        {
            var response = httpRequest.CreateResponse();
            await response.WriteAsJsonAsync(new ProcessUserClaimsResponse { Role = role });
            return response;
        }

        async Task<HttpResponseData> CreateErrorResponseAsync(string message)
        {
            var response = httpRequest.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync(message);
            return response;
        }
    }
}
