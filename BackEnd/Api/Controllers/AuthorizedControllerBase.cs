using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace OhMyWord.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
[RequiredScope("access")]
public abstract class AuthorizedControllerBase : ControllerBase
{
    protected ObjectResult GetErrorResult(int statusCode, string message) =>
        StatusCode(statusCode, new { message });
}
