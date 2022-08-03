using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
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
        [HttpTrigger("get", "post", Route = "get-roles/{userId:guid}")]
        HttpRequest request,
        Guid userId)
    {
        var player = await playerRepository.FindPlayerByPlayerIdAsync(userId);
        return player is not null
            ? new OkObjectResult(player)
            : new NotFoundResult();
    }
}
