using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OhMyWord.Data.Extensions;
using OhMyWord.Seeder.Services;

namespace OhMyWord.Seeder;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // services from data project
                services.AddDataServices(context.Configuration);                

                // local application services
                services.AddSingleton<DataReader>();
                services.AddHostedService<MainService>();
            })
            .Build();

        await host.RunAsync();
    }
}
