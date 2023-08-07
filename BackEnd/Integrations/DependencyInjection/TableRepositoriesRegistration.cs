using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OhMyWord.Infrastructure.Options;
using OhMyWord.Infrastructure.Services.Repositories;

namespace OhMyWord.Infrastructure.DependencyInjection;

public static class TableRepositoriesRegistration
{
    public static IServiceCollection AddTableRepositories(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<TableServiceOptions>()
            .Bind(configuration.GetSection(TableServiceOptions.SectionName))
            .Validate(TableServiceOptions.Validate, "Invalid TableService configuration")
            .ValidateOnStart();

        services.AddAzureClients(builder =>
        {
            var options = configuration.GetSection(TableServiceOptions.SectionName).Get<TableServiceOptions>();

            // use managed identity if endpoint specified, otherwise use connection string
            if (!string.IsNullOrEmpty(options?.Endpoint))
            {
                var uri = new Uri(options.Endpoint);
                builder.AddTableServiceClient(uri);
                builder.UseCredential(new DefaultAzureCredential());
            }
            else
            {
                builder.AddTableServiceClient(options?.ConnectionString!);
            }
        });

        services.AddSingleton<IUsersRepository, UsersRepository>();
        services.AddSingleton<IGeoLocationRepository, GeoLocationRepository>();

        return services;
    }
}
