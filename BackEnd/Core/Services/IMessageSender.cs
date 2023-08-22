using System.Net;

namespace OhMyWord.Core.Services;

public interface IMessageSender
{
    Task SendIpLookupMessageAsync(IPAddress ipAddress);
}
