using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OhMyWord.Integrations.RapidApi.Options;
using OhMyWord.Integrations.RapidApi.Services;
using Polly;
using Polly.Extensions.Http;
using System.Net;

namespace OhMyWord.Integrations.RapidApi.DependencyInjection;

public static class RapidApiClientsRegistration
{
    private static readonly IAsyncPolicy<HttpResponseMessage> Policy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(message => message.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

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
        }).AddPolicyHandler(Policy);

        services.AddHttpClient<IGeoLocationApiClient, GeoLocationApiClient>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<RapidApiOptions>>();
            httpClient.BaseAddress = new Uri("https://ip-geo-location.p.rapidapi.com/ip/");
            httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", options.Value.ApiKey);
        }).AddPolicyHandler(Policy);

        return services;
    }
}
