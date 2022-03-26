using MediatR;
using OhMyWord.Api.Mediator.Requests.Words;
using OhMyWord.Api.Responses.Words;
using OhMyWord.Data.Repositories;

namespace OhMyWord.Api.Handlers.Words;

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
