using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OhMyWord.Infrastructure.Services;

namespace OhMyWord.Infrastructure.DependencyInjection;

public static class TableRepositoriesRegistration
{
    public static IServiceCollection AddTableRepositories(this IServiceCollection services, HostBuilderContext context)
    {
        services.AddAzureClients(builder =>
        {
            if (context.HostingEnvironment.IsDevelopment())
            {
                // use local storage emulator
                builder.AddTableServiceClient("UseDevelopmentStorage=true");
            }
            else
            {
                // use managed identity
                builder.AddTableServiceClient(context.Configuration.GetSection("TableService"));
                builder.UseCredential(new DefaultAzureCredential());
            }
        });

        services.AddSingleton<IUsersRepository, UsersRepository>();
        services.AddSingleton<IGeoLocationRepository, GeoLocationRepository>();

        return services;
    }
}
