using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OhMyWord.Data.Services;
using System.Net;

namespace OhMyWord.Functions;

public sealed class UserFunctions
{
    private readonly ILogger<UserFunctions> logger;
    private readonly IVisitorRepository visitorRepository;

    public UserFunctions(ILogger<UserFunctions> logger, IVisitorRepository visitorRepository)
    {
        this.logger = logger;
        this.visitorRepository = visitorRepository;
    }

    [Function(nameof(GetUserRoles))]
    public async Task<HttpResponseData> GetUserRoles(
        [HttpTrigger("get", Route = "get-roles/{userId:guid}")]
        HttpRequestData request,
        string userId
    )
    {
        try
        {
            var visitor = await visitorRepository.GetVisitorAsync(userId);
            logger.LogInformation("Found visitor with ID: {UserId}", userId);
            var response = request.CreateResponse();
            await response.WriteAsJsonAsync(visitor);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Couldn't find visitor with ID: {UserId}", userId);
            return request.CreateResponse(HttpStatusCode.NotFound);
        }
    }
}
