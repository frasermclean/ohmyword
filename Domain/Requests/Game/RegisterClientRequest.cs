using MediatR;
using System.ComponentModel.DataAnnotations;
using WhatTheWord.Domain.Responses.Game;

namespace WhatTheWord.Domain.Requests.Game;

public class RegisterClientRequest : IRequest<RegisterClientResponse>
{
    [Required]
    public string Fingerprint { get; init; } = default!;
}
