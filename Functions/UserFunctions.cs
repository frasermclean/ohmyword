using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace OhMyWord.Functions;

public class UserFunctions
{
    [FunctionName(nameof(GetUserRoles))]
    public IActionResult GetUserRoles(
        [HttpTrigger(methods: "post", Route = "get-roles/{userId}")] HttpRequest request,
        string userId) 
    {
        return new OkObjectResult(userId);
    }
}
