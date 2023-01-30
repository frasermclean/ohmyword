using FastEndpoints;
using OhMyWord.Core.Responses.Words;
using OhMyWord.Data.Services;
using IMapper = AutoMapper.IMapper;

namespace OhMyWord.Api.Endpoints.Words.List;

public class ListWordsEndpoint : Endpoint<ListWordsRequest, ListWordsResponse>
{
    private readonly IWordsRepository wordsRepository;
    private readonly IMapper mapper;

    public ListWordsEndpoint(IWordsRepository wordsRepository, IMapper mapper)
    {
        this.wordsRepository = wordsRepository;
        this.mapper = mapper;
    }

    public override void Configure()
    {
        Get("words");
    }

    public override async Task<ListWordsResponse> ExecuteAsync(ListWordsRequest request,
        CancellationToken cancellationToken)
    {
        var (words, total) = await wordsRepository.GetWordsAsync(request.Offset, request.Limit,
            request.Filter ?? string.Empty, request.OrderBy, request.Direction, cancellationToken);

        return new ListWordsResponse
        {
            Offset = request.Offset,
            Limit = request.Limit,
            Total = total,
            Filter = request.Filter ?? string.Empty,
            OrderBy = request.OrderBy,
            Direction = request.Direction,
            Words = mapper.Map<IEnumerable<WordResponse>>(words)
        };
    }
}
