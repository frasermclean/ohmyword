using MediatR;
using WhatTheWord.Domain.Responses.Game;

namespace WhatTheWord.Domain.Requests.Game;

public class GetHintRequest : IRequest<GetHintResponse>
{
}
