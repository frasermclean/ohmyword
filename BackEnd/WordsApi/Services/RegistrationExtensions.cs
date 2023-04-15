using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OhMyWord.WordsApi.Options;

namespace OhMyWord.WordsApi.Services;

public static class RegistrationExtensions
{
    public static IServiceCollection AddWordsApiClient(this IServiceCollection services, HostBuilderContext context)
    {
        services.AddOptions<WordsApiOptions>()
            .Bind(context.Configuration.GetSection(WordsApiOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<IWordsApiClient, WordsApiClient>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<WordsApiOptions>>();
            httpClient.BaseAddress = new Uri("https://wordsapiv1.p.rapidapi.com/words/");
            httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", options.Value.ApiKey);
        });

        return services;
    }
}
