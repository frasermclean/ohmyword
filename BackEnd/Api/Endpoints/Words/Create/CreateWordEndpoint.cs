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
        var result = await wordsService.CreateWordAsync(new Word { Id = request.Id, Definitions = request.Definitions },
            cancellationToken);

        if (result.IsFailed)
        {
            AddError(result.Errors.First().Message);
            await SendErrorsAsync(StatusCodes.Status409Conflict, cancellationToken);
            return;
        }

        await SendCreatedAtAsync<GetWordEndpoint>(new { WordId = result.Value.Id }, result.Value,
            cancellation: cancellationToken);
    }
}
