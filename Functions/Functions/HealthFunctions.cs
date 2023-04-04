using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;

namespace OhMyWord.Functions.Functions;

public class HealthFunctions
{
    private readonly HealthCheckService healthCheckService;

    public HealthFunctions(HealthCheckService healthCheckService)
    {
        this.healthCheckService = healthCheckService;
    }

    [Function("GetHealth")]
    public async Task<HttpResponseData> GetHealthAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, Route = "health")] HttpRequestData request)
    {
        var report = await healthCheckService.CheckHealthAsync();
        return report.Status == HealthStatus.Healthy
            ? request.CreateResponse(HttpStatusCode.OK)
            : request.CreateResponse(HttpStatusCode.ServiceUnavailable);        
    }
}
