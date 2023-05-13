using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OhMyWord.Infrastructure.Services.RapidApi.WordsApi;

public class WordsApiClientHealthCheck : IHealthCheck
{
    private readonly IWordsApiClient wordsApiClient;

    public WordsApiClientHealthCheck(IWordsApiClient wordsApiClient)
    {
        this.wordsApiClient = wordsApiClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        await wordsApiClient.GetRandomWordDetailsAsync(cancellationToken);
        return HealthCheckResult.Healthy();
    }
}
