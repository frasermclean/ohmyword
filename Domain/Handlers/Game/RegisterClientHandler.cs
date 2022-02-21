using MediatR;
using WhatTheWord.Domain.Requests.Game;
using WhatTheWord.Domain.Responses.Game;

namespace WhatTheWord.Domain.Handlers.Game;

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
