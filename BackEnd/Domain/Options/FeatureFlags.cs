namespace OhMyWord.Domain.Options;

public static class FeatureFlags
{
    /// <summary>
    /// Configuration section name for feature flags.
    /// </summary>
    public const string SectionName = "FeatureFlags";
    
    /// <summary>
    /// Application will dispatch IP lookup requests to Service Bus queue.
    /// </summary>
    public const string IpLookup = "IpLookup";
}
