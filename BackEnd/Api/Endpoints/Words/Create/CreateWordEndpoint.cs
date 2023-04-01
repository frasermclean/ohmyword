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
        var result = request.Definitions.Any()
            ? await wordsService.CreateWordAsync(new Word { Id = request.Id, Definitions = request.Definitions },
                cancellationToken)
            : await wordsService.CreateWordAsync(request.Id, cancellationToken);

        await result.Match(
            word => SendCreatedAtAsync<GetWordEndpoint>(new { WordId = word.Id }, word,
                cancellation: cancellationToken),
            _ =>
            {
                AddError($"Could not find the word: '{request.Id}' in the dictionary.");
                return SendErrorsAsync(cancellation: cancellationToken);
            },
            conflict =>
            {
                AddError(conflict.Message);
                return SendErrorsAsync(StatusCodes.Status409Conflict, cancellationToken);
            });
    }
}
