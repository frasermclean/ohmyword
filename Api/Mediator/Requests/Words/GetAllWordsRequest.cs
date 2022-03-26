using MediatR;
using WhatTheWord.Api.Responses.Words;

namespace WhatTheWord.Api.Mediator.Requests.Words;

public class GetAllWordsRequest : IRequest<IEnumerable<WordResponse>>
{
}
