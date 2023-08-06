using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace OhMyWord.Api.Endpoints.Words.Create;

[FastEndpoints.HttpPost("words")]
public class CreateWordEndpoint : Endpoint<CreateWordRequest, Results<Created<Word>, Conflict>>
{
    private readonly IWordsService wordsService;

    public CreateWordEndpoint(IWordsService wordsService)
    {
        this.wordsService = wordsService;
    }

    public override async Task<Results<Created<Word>, Conflict>> ExecuteAsync(CreateWordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await wordsService.CreateWordAsync(
            new Word { Id = request.Id, Definitions = request.Definitions, Frequency = request.Frequency },
            cancellationToken);

        return result.IsSuccess
            ? Created($"/words/{result.Value.Id}", result.Value)
            : Conflict();
    }
}
