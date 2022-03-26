using MediatR;
using OhMyWord.Api.Mediator.Requests.Game;
using OhMyWord.Api.Responses.Game;

namespace OhMyWord.Api.Mediator.Handlers.Game;

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
