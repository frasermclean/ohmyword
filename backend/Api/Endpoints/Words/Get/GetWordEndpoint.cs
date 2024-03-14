using Microsoft.AspNetCore.Http.HttpResults;
using OhMyWord.Api.Models;
using OhMyWord.Domain.Services;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace OhMyWord.Api.Endpoints.Words.Get;

[HttpGet("/words/{wordId}")]
public class GetWordEndpoint : Endpoint<GetWordRequest, Results<Ok<WordResponse>, NotFound>>
{
    private readonly IWordsService wordsService;

    public GetWordEndpoint(IWordsService wordsService)
    {
        this.wordsService = wordsService;
    }

    public override async Task<Results<Ok<WordResponse>, NotFound>> ExecuteAsync(GetWordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await wordsService.GetWordAsync(request.WordId, request.PerformExternalLookup.GetValueOrDefault(),
            cancellationToken);

        return result.IsSuccess
            ? Ok(WordResponse.FromWord(result.Value))
            : NotFound();
    }
}
