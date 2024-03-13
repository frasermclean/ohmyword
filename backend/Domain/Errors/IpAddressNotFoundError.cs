using FluentResults;

namespace OhMyWord.Domain.Errors;

public class IpAddressNotFoundError : Error
{
    public IpAddressNotFoundError(string ipAddress)
        : base($"IP address not found: {ipAddress}")
    {
    }
}
