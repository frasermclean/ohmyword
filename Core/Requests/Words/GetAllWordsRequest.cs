using MediatR;
using OhMyWord.Core.Responses.Words;

namespace OhMyWord.Core.Requests.Words;

public class GetAllWordsRequest : IRequest<IEnumerable<WordResponse>>
{    
}