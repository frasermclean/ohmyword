using FluentResults;

namespace OhMyWord.Integrations.Errors;

public class InvalidIpAddressError : Error
{
    public InvalidIpAddressError(string ipAddress)
        : base($"Invalid IP address: {ipAddress}")
    {
    }
}
