﻿using Azure.Core.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Extensions;
using OhMyWord.Infrastructure.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OhMyWord.Functions;

public static class Program
{
    public static void Main()
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults(options =>
            {
                options.Serializer = new JsonObjectSerializer(new JsonSerializerOptions
                {
                    IgnoreReadOnlyProperties = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                });
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IUsersService, UsersService>();
                services.AddSingleton<IUsersRepository, UsersRepository>();
                services.AddInfrastructureServices(context);
                
                // health checks
                services.AddHealthChecks();
                //.AddAzureTable();
            })
            .Build();

        host.Run();
    }
}
