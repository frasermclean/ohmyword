using MediatR;

namespace WhatTheWord.Domain.Processing.Clients;

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
