using FastEndpoints;
using OhMyWord.Core.Models;
using OhMyWord.Core.Services;

namespace OhMyWord.Api.Endpoints.Words.List;

public class ListWordsEndpoint : Endpoint<ListWordsRequest, IEnumerable<Word>>
{
    private readonly IWordsService wordsService;

    public ListWordsEndpoint(IWordsService wordsService)
    {
        this.wordsService = wordsService;
    }

    public override void Configure()
    {
        Get("words");
    }

    public override async Task<IEnumerable<Word>> ExecuteAsync(ListWordsRequest request,
        CancellationToken cancellationToken)
    {
        return await wordsService.GetWordsAsync(request.Offset, request.Limit,
            request.Filter, request.OrderBy, request.Direction, cancellationToken);
    }
}
