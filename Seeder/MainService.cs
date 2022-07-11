using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OhMyWord.Services.Data;

namespace OhMyWord.Seeder;

internal class MainService : BackgroundService
{
    private readonly ILogger<MainService> logger;
    private readonly ICosmosDbService cosmosDbService;

    public MainService(ILogger<MainService> logger, ICosmosDbService cosmosDbService)
    {
        this.logger = logger;
        this.cosmosDbService = cosmosDbService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
