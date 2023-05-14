using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OhMyWord.Infrastructure.Options;
using OhMyWord.Infrastructure.Services.RapidApi.IpGeoLocation;
using OhMyWord.Infrastructure.Services.RapidApi.WordsApi;

namespace OhMyWord.Infrastructure.Services.RapidApi;

public static class RapidApiClientsRegistration
{
    public static IServiceCollection AddRapidApiServices(this IServiceCollection services)
    {
        services.AddOptions<RapidApiOptions>()
            .BindConfiguration(RapidApiOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IWordsApiClient, WordsApiClient>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<RapidApiOptions>>();
            httpClient.BaseAddress = new Uri("https://wordsapiv1.p.rapidapi.com/words/");
            httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", options.Value.ApiKey);
        });

        services.AddHttpClient<IGeoLocationApiClient, GeoLocationApiClient>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<RapidApiOptions>>();
            httpClient.BaseAddress = new Uri("https://ip-geo-location.p.rapidapi.com/ip/");
            httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", options.Value.ApiKey);
        });

        return services;
    }
}
