using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Integrations.ServiceBus.Options;

public class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    [Required] public string Namespace { get; set; } = string.Empty;
    [Required] public string IpLookupQueueName { get; set; } = string.Empty;
}
