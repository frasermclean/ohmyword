﻿using FastEndpoints;
using OhMyWord.Api.Services;
using OhMyWord.Core.Models;

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
        var word = await wordsService.GetWordAsync(request.WordId, cancellationToken);

        if (word is null)
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        await SendOkAsync(word, cancellationToken);
    }
}
