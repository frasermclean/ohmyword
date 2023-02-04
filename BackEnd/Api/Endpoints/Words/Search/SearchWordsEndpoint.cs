using FastEndpoints;
using OhMyWord.Api.Models;
using OhMyWord.Api.Services;

namespace OhMyWord.Api.Endpoints.Words.Search;

public class SearchWordsEndpoint : Endpoint<SearchWordsRequest, IEnumerable<Word>>
{
    private readonly IWordsService wordsService;

    public SearchWordsEndpoint(IWordsService wordsService)
    {
        this.wordsService = wordsService;
    }

    public override void Configure()
    {
        Get("words");
    }

    public override async Task<IEnumerable<Word>> ExecuteAsync(SearchWordsRequest request,
        CancellationToken cancellationToken)
    {
        return await wordsService
            .SearchWordsAsync(request.Offset, request.Limit, request.Filter, request.OrderBy, request.Direction,
                cancellationToken)
            .ToListAsync(cancellationToken);
    }
}
