using OhMyWord.Api.Startup;
using Serilog;

namespace OhMyWord.Api;

public class Program
{
    public static void Main(string[] args)
    {
        // create serilog bootstrap logger
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting application");

            // build the application and configure the pipeline
            var app = WebApplication.CreateBuilder(args)
                .AddAzureAppConfiguration()
                .ConfigureAndBuildHost()
                .ConfigurePipeline();

            // run the application
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
