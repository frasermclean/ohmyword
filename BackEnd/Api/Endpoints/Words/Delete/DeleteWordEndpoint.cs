using Microsoft.AspNetCore.Http.HttpResults;
using OhMyWord.Domain.Services;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace OhMyWord.Api.Endpoints.Words.Delete;

[HttpDelete("/words/{wordId}")]
public class DeleteWordEndpoint : Endpoint<DeleteWordRequest, Results<NoContent, NotFound>>
{
    private readonly IWordsService wordsService;

    public DeleteWordEndpoint(IWordsService wordsService)
    {
        this.wordsService = wordsService;
    }

    public override async Task<Results<NoContent, NotFound>> ExecuteAsync(DeleteWordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await wordsService.DeleteWordAsync(request.WordId, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : NotFound();
    }
}
