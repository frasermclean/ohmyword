using MediatR;
using WhatTheWord.Api.Mediator.Requests.Game;
using WhatTheWord.Api.Responses.Game;

namespace WhatTheWord.Api.Mediator.Handlers.Game;

public class RegisterClientHandler : IRequestHandler<RegisterClientRequest, RegisterClientResponse>
{
    public Task<RegisterClientResponse> Handle(RegisterClientRequest request, CancellationToken cancellationToken)
    {
        var response = new RegisterClientResponse()
        {
            ClientId = Guid.NewGuid().ToString(),
        };

        return Task.FromResult(response);
    }
}
