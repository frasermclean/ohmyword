using Microsoft.AspNetCore.Http.HttpResults;
using OhMyWord.Api.Extensions;
using OhMyWord.Api.Models;
using OhMyWord.Core.Models;
using OhMyWord.Domain.Services;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace OhMyWord.Api.Endpoints.Words.Create;

[HttpPost("words")]
public class CreateWordEndpoint : Endpoint<CreateWordRequest, Results<Created<WordResponse>, Conflict>>
{
    private readonly IWordsService wordsService;

    public CreateWordEndpoint(IWordsService wordsService)
    {
        this.wordsService = wordsService;
    }

    public override async Task<Results<Created<WordResponse>, Conflict>> ExecuteAsync(CreateWordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await wordsService.CreateWordAsync(
            new Word
            {
                Id = request.Id,
                Definitions = request.Definitions,
                Frequency = request.Frequency,
                LastModifiedBy = HttpContext.User.GetUserId()
            },
            cancellationToken);

        return result.IsSuccess
            ? Created($"/words/{result.Value.Id}", WordResponse.FromWord(result.Value))
            : Conflict();
    }
}
