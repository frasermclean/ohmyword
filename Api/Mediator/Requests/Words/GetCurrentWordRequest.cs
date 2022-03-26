using MediatR;
using OhMyWord.Api.Responses.Words;

namespace OhMyWord.Api.Mediator.Requests.Words;

public class GetCurrentWordRequest : IRequest<CurrentWordResponse>
{
}
