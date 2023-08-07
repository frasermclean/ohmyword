using Microsoft.AspNetCore.Http.HttpResults;
using OhMyWord.Api.Extensions;
using OhMyWord.Core.Models;
using OhMyWord.Domain.Services;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace OhMyWord.Api.Endpoints.Words.Update;

[HttpPut("/words/{wordId}")]
public class UpdateWordEndpoint : Endpoint<UpdateWordRequest, Results<Ok<Word>, NotFound>>
{
    private readonly IWordsService wordsService;

    public UpdateWordEndpoint(IWordsService wordsService)
    {
        this.wordsService = wordsService;
    }

    public override async Task<Results<Ok<Word>, NotFound>> ExecuteAsync(UpdateWordRequest request,
        CancellationToken cancellationToken)
    {
        var word = new Word
        {
            Id = request.WordId,
            Definitions = request.Definitions,
            Frequency = request.Frequency,
            LastModifiedBy = HttpContext.User.GetUserId()
        };

        var result = await wordsService.UpdateWordAsync(word, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound();
    }
}
