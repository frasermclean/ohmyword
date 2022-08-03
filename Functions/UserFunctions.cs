using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using OhMyWord.Data.Services;
using System.Threading.Tasks;

namespace OhMyWord.Functions;

public sealed class UserFunctions
{
    private readonly IPlayerRepository playerRepository;

    public UserFunctions(IPlayerRepository playerRepository)
    {
        this.playerRepository = playerRepository;
    }

    [FunctionName(nameof(GetUserRoles))]
    public Task<IActionResult> GetUserRoles(
        [HttpTrigger("get", "post", Route = "get-roles/{userId}")]
        HttpRequest request,
        string userId)
    {
        return Task.FromResult<IActionResult>(new OkObjectResult(new { userId, }));
    }
}
