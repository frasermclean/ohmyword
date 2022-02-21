using MediatR;
using WhatTheWord.Domain.Requests.Game;
using WhatTheWord.Domain.Responses.Game;

namespace WhatTheWord.Domain.Handlers.Game;

public class GetCurrentWordHandler : IRequestHandler<GetHintRequest, GetHintResponse>
{
    public Task<GetHintResponse> Handle(GetHintRequest request, CancellationToken cancellationToken)
    {
        var response = new GetHintResponse();
        return Task.FromResult(response);
    }
}
