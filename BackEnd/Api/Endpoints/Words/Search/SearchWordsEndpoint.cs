using OhMyWord.Domain.Services;

namespace OhMyWord.Api.Endpoints.Words.Search;

public class SearchWordsEndpoint : Endpoint<SearchWordsRequest, SearchWordsResponse>
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

    public override async Task<SearchWordsResponse> ExecuteAsync(SearchWordsRequest request,
        CancellationToken cancellationToken)
    {
        var totalTask = wordsService.GetTotalWordCountAsync(cancellationToken);
        var wordsTask = wordsService
            .SearchWords(request.Offset, request.Limit, request.Filter, request.OrderBy, request.Direction,
                cancellationToken)
            .ToListAsync(cancellationToken)
            .AsTask();

        await Task.WhenAll(totalTask, wordsTask);

        return new SearchWordsResponse { Total = totalTask.Result, Words = wordsTask.Result };
    }
}
