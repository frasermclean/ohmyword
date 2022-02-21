using MediatR;
using WhatTheWord.Data.Repositories;
using WhatTheWord.Domain.Requests.Words;
using WhatTheWord.Domain.Responses.Words;

namespace WhatTheWord.Domain.Handlers.Words;

public class GetAllWordsHandler : IRequestHandler<GetAllWordsRequest, IEnumerable<WordResponse>>
{
    private readonly IWordsRepository wordsRepository;

    public GetAllWordsHandler(IWordsRepository wordsRepository)
    {
        this.wordsRepository = wordsRepository;
    }

    public async Task<IEnumerable<WordResponse>> Handle(GetAllWordsRequest request, CancellationToken cancellationToken)
    {
        var words = await wordsRepository.GetAllWordsAsync();
        var response = words.Select(word => new WordResponse(word));
        return response;
    }
}
