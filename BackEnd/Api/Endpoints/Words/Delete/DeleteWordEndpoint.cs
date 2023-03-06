using OhMyWord.Domain.Services;

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
        await wordsService.DeleteWordAsync(request.WordId, cancellationToken);
    }
}
