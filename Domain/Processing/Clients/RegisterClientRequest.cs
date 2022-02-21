using MediatR;
using System.ComponentModel.DataAnnotations;

namespace WhatTheWord.Domain.Processing.Clients;

public class RegisterClientRequest : IRequest<RegisterClientResponse>
{
    [Required]
    public string Fingerprint { get; init; } = default!;
}
