using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OhMyWord.Data.Services;

namespace OhMyWord.Seeder;

internal class MainService : BackgroundService
{
    private readonly ILogger<MainService> logger;
    private readonly IWordsRepository wordsRepository;
    private readonly DataReader dataReader;

    public MainService(ILogger<MainService> logger, IWordsRepository wordsRepository, DataReader dataReader)
    {
        this.logger = logger;
        this.wordsRepository = wordsRepository;
        this.dataReader = dataReader;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var wordCreationCount = 0;
        foreach (var word in dataReader.Words)
        {
            try
            {
                await wordsRepository.CreateWordAsync(word);
                wordCreationCount++;
                logger.LogInformation("Successfully created word: {Word}", word);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error occurred while trying to create word: {Word}", word);
            }
        }

        logger.LogInformation("All operations completed. Created {Count} words", wordCreationCount);
    }
}
