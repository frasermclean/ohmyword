using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using OhMyWord.Core.Services;

namespace OhMyWord.Integrations.GraphApi.Services;

public class GraphApiClient : IGraphApiClient
{
    private readonly ILogger<GraphApiClient> logger;
    private readonly GraphServiceClient client;

    public GraphApiClient(ILogger<GraphApiClient> logger, GraphServiceClient client)
    {
        this.logger = logger;
        this.client = client;
    }

    public async Task<(string? Firstname, string? Lastname, string? DisplayName)> GetUserDetailsAsync(Guid userId,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Attempting to get user with ID: {UserId} from Graph API", userId);

        var user = await client.Users[userId.ToString()].GetAsync(cancellationToken: cancellationToken);
        return (user?.GivenName, user?.Surname, user?.DisplayName);
    }
}
