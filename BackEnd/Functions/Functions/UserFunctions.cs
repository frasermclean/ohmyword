using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OhMyWord.Data.Entities;
using OhMyWord.Data.Services;
using OhMyWord.Functions.Models;
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

    [Function(nameof(GetUserClaims))]
    public async Task<HttpResponseData> GetUserClaims(
        [HttpTrigger("post", Route = "get-user-claims")]
        HttpRequestData httpRequest
    )
    {
        try
        {
            var getUserClaimsRequest = await httpRequest.ReadFromJsonAsync<GetUserClaimsRequest>();
            var role = getUserClaimsRequest?.UserId == "69a85c34-9566-40ba-b43c-ffcf664f717d"
                ? Role.Admin
                : Role.User;

            logger.LogInformation("Incoming user ID is is: {UserId}, determined role is: {Role}",
                getUserClaimsRequest?.UserId, role);

            var response = httpRequest.CreateResponse();
            await response.WriteAsJsonAsync(new GetUserClaimsResponse
            {
                Role = role
            });
            return response;
        }
        catch (Exception ex)
        {
            // logger.LogWarning(ex, "Couldn't find visitor with ID: {UserId}", userId);
            return httpRequest.CreateResponse(HttpStatusCode.NotFound);
        }
    }
}
