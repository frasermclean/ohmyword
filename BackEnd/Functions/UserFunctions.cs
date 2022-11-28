using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OhMyWord.Data.Services;
using System.Net;

namespace OhMyWord.Functions;

public sealed class UserFunctions
{
    private readonly ILogger<UserFunctions> logger;
    private readonly IPlayerRepository playerRepository;

    public UserFunctions(ILogger<UserFunctions> logger, IPlayerRepository playerRepository)
    {
        this.logger = logger;
        this.playerRepository = playerRepository;
    }

    [Function(nameof(GetUserRoles))]
    public async Task<HttpResponseData> GetUserRoles(
        [HttpTrigger("get", Route = "get-roles/{userId:guid}")]
        HttpRequestData request,
        Guid userId
    )
    {
        try
        {
            var player = await playerRepository.FindPlayerByPlayerIdAsync(userId);
            logger.LogInformation("Found player with ID: {UserId}", userId);
            var response = request.CreateResponse();
            await response.WriteAsJsonAsync(player);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Couldn't find player with ID: {UserId}", userId);
            return request.CreateResponse(HttpStatusCode.NotFound);
        }
    }
}
