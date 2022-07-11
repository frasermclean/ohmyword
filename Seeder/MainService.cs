using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OhMyWord.Services.Data;

namespace OhMyWord.Seeder;

internal class MainService : BackgroundService
{
    private readonly ILogger<MainService> logger;
    private readonly ICosmosDbService cosmosDbService;
    private readonly DataReader dataReader;

    public MainService(ILogger<MainService> logger, ICosmosDbService cosmosDbService, DataReader dataReader)
    {
        this.logger = logger;
        this.cosmosDbService = cosmosDbService;
        this.dataReader = dataReader;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
