﻿namespace OhMyWord.Functions.Models;

public record ProcessUserClaimsRequest
{
    public string IdentityProvider { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
