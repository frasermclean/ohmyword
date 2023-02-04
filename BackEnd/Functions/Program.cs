using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OhMyWord.Data.Extensions;
using OhMyWord.Data.Options;

namespace OhMyWord.Functions;

public static class Program
{
    public static void Main()
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()            
            .ConfigureServices((context, services) =>
            {
               // data services
                services.AddDataServices(context.Configuration);

                // health checks
                var cosmosDbOptions = context.Configuration
                    .GetSection(CosmosDbOptions.SectionName)
                    .Get<CosmosDbOptions>();
                services.AddHealthChecks()
                    .AddCosmosDb(cosmosDbOptions?.ConnectionString ?? string.Empty);
            })
            .Build();

        host.Run();
    }
}
