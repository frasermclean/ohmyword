using FluentResults;

namespace OhMyWord.Data.CosmosDb.Errors;

public class InvalidIpAddressError : Error
{
    public InvalidIpAddressError(string ipAddress)
        : base($"Invalid IP address: {ipAddress}")
    {
    }
}
