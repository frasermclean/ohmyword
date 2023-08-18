using Microsoft.AspNetCore.Http.HttpResults;
using OhMyWord.Api.Extensions;
using OhMyWord.Api.Models;
using OhMyWord.Core.Models;
using OhMyWord.Domain.Services;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace OhMyWord.Api.Endpoints.Words.Update;

[HttpPut("/words/{wordId}")]
public class UpdateWordEndpoint : Endpoint<UpdateWordRequest, Results<Ok<WordResponse>, NotFound>>
{
    private readonly IWordsService wordsService;

    public UpdateWordEndpoint(IWordsService wordsService)
    {
        this.wordsService = wordsService;
    }

    public override async Task<Results<Ok<WordResponse>, NotFound>> ExecuteAsync(UpdateWordRequest request,
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
            ? Ok(WordResponse.FromWord(result.Value))
            : NotFound();
    }
}
