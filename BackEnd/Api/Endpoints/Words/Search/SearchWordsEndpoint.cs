﻿using OhMyWord.Api.Models;
using OhMyWord.Domain.Services;

namespace OhMyWord.Api.Endpoints.Words.Search;

[HttpGet("words")]
public class SearchWordsEndpoint : Endpoint<SearchWordsRequest, SearchWordsResponse>
{
    private readonly IWordsService wordsService;

    public SearchWordsEndpoint(IWordsService wordsService)
    {
        this.wordsService = wordsService;
    }

    public override async Task<SearchWordsResponse> ExecuteAsync(SearchWordsRequest request,
        CancellationToken cancellationToken)
    {
        var totalTask = wordsService.GetTotalWordCountAsync(cancellationToken);
        var wordsTask = wordsService
            .SearchWords(request.Offset, request.Limit, request.Filter, request.OrderBy, request.IsDescending,
                cancellationToken)
            .ToListAsync(cancellationToken)
            .AsTask();

        await Task.WhenAll(totalTask, wordsTask);

        return new SearchWordsResponse
        {
            Total = totalTask.Result, Words = wordsTask.Result.Select(WordResponse.FromWord)
        };
    }
}
