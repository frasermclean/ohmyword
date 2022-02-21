using MediatR;
using WhatTheWord.Domain.Responses.Words;

namespace WhatTheWord.Domain.Requests.Words;

public class GetAllWordsRequest : IRequest<IEnumerable<WordResponse>>
{
}
