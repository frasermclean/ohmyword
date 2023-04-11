using Microsoft.Extensions.Diagnostics.HealthChecks;
using OhMyWord.WordsApi.Services;

namespace OhMyWord.WordsApi.HealthChecks;

public class WordsApiClientHealthCheck : IHealthCheck
{
    private readonly IWordsApiClient wordsApiClient;

    public WordsApiClientHealthCheck(IWordsApiClient wordsApiClient)
    {
        this.wordsApiClient = wordsApiClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var details = await wordsApiClient.GetWordDetailsAsync("health", cancellationToken);
        return details is not null ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
    }
}
