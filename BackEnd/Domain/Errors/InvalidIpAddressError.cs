using FluentResults;

namespace OhMyWord.Domain.Errors;

public class InvalidIpAddressError : Error
{
    public InvalidIpAddressError(string ipAddress)
        : base($"Invalid IP address: {ipAddress}")
    {
    }
}
