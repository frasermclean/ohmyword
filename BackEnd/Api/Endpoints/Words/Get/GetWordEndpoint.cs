using OhMyWord.Domain.Models;
using OhMyWord.Domain.Services;

namespace OhMyWord.Api.Endpoints.Words.Get;

public class GetWordEndpoint : Endpoint<GetWordRequest, Word>
{
    private readonly IWordsService wordsService;

    public GetWordEndpoint(IWordsService wordsService)
    {
        this.wordsService = wordsService;
    }

    public override void Configure()
    {
        Get("/words/{wordId}");
    }

    public override async Task HandleAsync(GetWordRequest request, CancellationToken cancellationToken)
    {
        var result = await wordsService.GetWordAsync(request.WordId, cancellationToken);
        
        await (result.IsSuccess
            ? SendOkAsync(result.Value, cancellationToken)
            : SendNotFoundAsync(cancellationToken));
    }
}
