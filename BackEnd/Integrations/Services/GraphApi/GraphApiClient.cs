using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace OhMyWord.Infrastructure.Services.GraphApi;

public interface IGraphApiClient
{
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class GraphApiClient : IGraphApiClient
{
    private readonly ILogger<GraphApiClient> logger;
    private readonly GraphServiceClient client;

    public GraphApiClient(ILogger<GraphApiClient> logger, GraphServiceClient client)
    {
        this.logger = logger;
        this.client = client;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Attempting to get user with ID: {UserId} from Graph API", userId);
        return await client.Users[userId.ToString()].GetAsync(cancellationToken: cancellationToken);
    }
}
