using OhMyWord.Api.Endpoints.Words.Get;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;

namespace OhMyWord.Api.Endpoints.Words.Create;

public class CreateWordEndpoint : Endpoint<CreateWordRequest, Word>
{
    private readonly IWordsService wordsService;

    public CreateWordEndpoint(IWordsService wordsService)
    {
        this.wordsService = wordsService;
    }

    public override void Configure()
    {
        Post("words");
    }

    public override async Task HandleAsync(CreateWordRequest request, CancellationToken cancellationToken)
    {
        var word = new Word { Id = request.Id, Definitions = request.Definitions };
        await wordsService.CreateWordAsync(word, cancellationToken);

        await SendCreatedAtAsync<GetWordEndpoint>(new { WordId = word.Id }, word, cancellation: cancellationToken);
    }
}
