using System.ComponentModel.DataAnnotations;
using MediatR;
using OhMyWord.Api.Responses.Game;

namespace OhMyWord.Api.Mediator.Requests.Game;

public class RegisterPlayerRequest : IRequest<RegisterPlayerResponse>
{
    [Required]
    public string VisitorId { get; init; } = default!;

    public string ConnectionId { get; set; } = string.Empty;
}
