using MediatR;
using WhatTheWord.Data.Repositories;

namespace WhatTheWord.Domain.Processing.Words;

public class GetAllWordsHandler : IRequestHandler<GetAllWordsRequest, GetAllWordsResponse>
{
    private readonly IWordsRepository wordsRepository;

    public GetAllWordsHandler(IWordsRepository wordsRepository)
    {
        this.wordsRepository = wordsRepository;
    }

    public async Task<GetAllWordsResponse> Handle(GetAllWordsRequest request, CancellationToken cancellationToken)
    {
        var words = await wordsRepository.GetAllWordsAsync();
        return new GetAllWordsResponse(words);
    }
}
