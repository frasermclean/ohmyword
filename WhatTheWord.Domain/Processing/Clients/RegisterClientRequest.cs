using MediatR;

namespace WhatTheWord.Domain.Processing.Clients;

public class RegisterClientRequest : IRequest<RegisterClientResponse>
{
    public string Fingerprint { get; init; } = default!;
}
