using FastEndpoints;
using OhMyWord.Api.Models;
using OhMyWord.Api.Services;

namespace OhMyWord.Api.Endpoints.Words.Update;

public class UpdateWordEndpoint : Endpoint<UpdateWordRequest, Word>
{
    private readonly IWordsService wordsService;

    public UpdateWordEndpoint(IWordsService wordsService)
    {
        this.wordsService = wordsService;
    }

    public override void Configure()
    {
        Put("words/{wordId}");
    }

    public override async Task HandleAsync(UpdateWordRequest request, CancellationToken cancellationToken)
    {
        var word = new Word { Id = request.WordId, Definitions = request.Definitions, LastModified = DateTime.UtcNow };
        await wordsService.UpdateWordAsync(word, cancellationToken);
        await SendOkAsync(word, cancellationToken);
    }
}
