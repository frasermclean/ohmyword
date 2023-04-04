using Microsoft.Extensions.Diagnostics.HealthChecks;
using OhMyWord.Infrastructure.Services;

namespace OhMyWord.Infrastructure.HealthChecks;

public class DictionaryServiceHealthCheck : IHealthCheck
{
    private readonly IDictionaryService dictionaryService;

    public DictionaryServiceHealthCheck(IDictionaryService dictionaryService)
    {
        this.dictionaryService = dictionaryService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var words = await dictionaryService.LookupWordAsync("health", cancellationToken);
        return words.Any() ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
    }
}
