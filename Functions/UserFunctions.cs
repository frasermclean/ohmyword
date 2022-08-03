using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using OhMyWord.Data.Services;

namespace OhMyWord.Functions;

public sealed class UserFunctions
{
    private readonly IPlayerRepository playerRepository;

    public UserFunctions(IPlayerRepository playerRepository)
    {
        this.playerRepository = playerRepository;
    }

    [FunctionName(nameof(GetUserRoles))]
    public async Task<IActionResult> GetUserRoles(
        [HttpTrigger("get", Route = "get-roles/{userId:guid}")]
        HttpRequest request,
        Guid userId,
        ILogger logger)
    {
        try
        {
            var player = await playerRepository.FindPlayerByPlayerIdAsync(userId);
            logger.LogInformation("Found player with ID: {UserId}", userId);
            return new OkObjectResult(player);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Couldn't find player with ID: {UserId}", userId);
            return new NotFoundResult();
        }
    }
}
