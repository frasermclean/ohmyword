using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OhMyWord.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public abstract class AuthorizedControllerBase : ControllerBase
{
    protected ObjectResult GetErrorResult(int statusCode, string message) =>
        StatusCode(statusCode, new { message });
}
