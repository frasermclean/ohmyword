using FluentResults;

namespace OhMyWord.Infrastructure.Errors;

public class InvalidIpAddressError : Error
{
    public InvalidIpAddressError(string ipAddress)
        : base($"Invalid IP address: {ipAddress}")
    {
    }
}
