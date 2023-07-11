using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OhMyWord.Infrastructure.Services.Repositories;

namespace OhMyWord.Seeder.Services;

internal class MainService : BackgroundService
{
    private readonly ILogger<MainService> logger;
    private readonly IWordsRepository wordsRepository;
    private readonly IDefinitionsRepository definitionsRepository;
    private readonly DataReader dataReader;

    public MainService(ILogger<MainService> logger, DataReader dataReader, IWordsRepository wordsRepository,
        IDefinitionsRepository definitionsRepository)
    {
        this.logger = logger;
        this.dataReader = dataReader;
        this.wordsRepository = wordsRepository;
        this.definitionsRepository = definitionsRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var wordCreationCount = 0;
        foreach (var word in dataReader.Words)
        {
            var definitions = dataReader.GetDefinitions(word.Id);
            try
            {
                await wordsRepository.CreateWordAsync(word);
                foreach (var definition in definitions)
                {
                    await definitionsRepository.CreateDefinitionAsync(definition);
                }

                wordCreationCount++;
                logger.LogInformation("Successfully created word: {WordId}", word);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error occurred while trying to create word: {WordId}", word);
            }
        }

        logger.LogInformation("All operations completed. Created {Count} words", wordCreationCount);
    }
}
