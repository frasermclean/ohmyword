namespace OhMyWord.Data.CosmosDb.Options;

public class CosmosDbOptions
{
    /// <summary>
    /// Section name for the app configuration.
    /// </summary>
    public const string SectionName = "CosmosDb";

    public string ConnectionString { get; init; } = string.Empty;
    public string AccountEndpoint { get; init; } = string.Empty;
    public string DatabaseId { get; init; } = string.Empty;

    public string ApplicationName { get; init; } = "OhMyWord API";

    public static bool Validate(CosmosDbOptions options)
        => !(string.IsNullOrEmpty(options.ConnectionString) && string.IsNullOrEmpty(options.AccountEndpoint))
           && !string.IsNullOrEmpty(options.DatabaseId);
}
