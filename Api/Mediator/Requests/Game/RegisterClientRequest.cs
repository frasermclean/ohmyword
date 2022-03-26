using System.ComponentModel.DataAnnotations;
using MediatR;
using WhatTheWord.Api.Responses.Game;

namespace WhatTheWord.Api.Mediator.Requests.Game;

public class RegisterClientRequest : IRequest<RegisterClientResponse>
{
    [Required]
    public string VisitorId { get; init; } = default!;
}
