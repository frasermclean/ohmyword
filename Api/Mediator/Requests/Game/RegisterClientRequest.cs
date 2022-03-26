using System.ComponentModel.DataAnnotations;
using MediatR;
using OhMyWord.Api.Responses.Game;

namespace OhMyWord.Api.Mediator.Requests.Game;

public class RegisterClientRequest : IRequest<RegisterClientResponse>
{
    [Required]
    public string VisitorId { get; init; } = default!;
}
