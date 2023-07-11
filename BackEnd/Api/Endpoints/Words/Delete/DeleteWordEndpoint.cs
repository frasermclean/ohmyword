using OhMyWord.Domain.Services;
using OhMyWord.Infrastructure.Errors;

namespace OhMyWord.Api.Endpoints.Words.Delete;

public class DeleteWordEndpoint : Endpoint<DeleteWordRequest>
{
    private readonly IWordsService wordsService;

    public DeleteWordEndpoint(IWordsService wordsService)
    {
        this.wordsService = wordsService;
    }

    public override void Configure()
    {
        Delete("words/{wordId}");
    }

    public override async Task HandleAsync(DeleteWordRequest request, CancellationToken cancellationToken)
    {
        var result = await wordsService.DeleteWordAsync(request.WordId, cancellationToken);

        if (result.HasError<ItemNotFoundError>())
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        await SendNoContentAsync(cancellationToken);
    }
}
