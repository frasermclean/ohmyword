using Microsoft.AspNetCore.Http.HttpResults;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace OhMyWord.Api.Endpoints.Words.Get;

[HttpGet("/words/{wordId}")]
public class GetWordEndpoint : Endpoint<GetWordRequest, Results<Ok<Word>, NotFound>>
{
    private readonly IWordsService wordsService;

    public GetWordEndpoint(IWordsService wordsService)
    {
        this.wordsService = wordsService;
    }

    public override async Task<Results<Ok<Word>, NotFound>> ExecuteAsync(GetWordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await wordsService.GetWordAsync(request.WordId, request.PerformExternalLookup.GetValueOrDefault(),
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound();
    }
}
