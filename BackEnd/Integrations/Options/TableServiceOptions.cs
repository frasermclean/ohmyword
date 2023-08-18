namespace OhMyWord.Integrations.Options;

public class TableServiceOptions
{
    public const string SectionName = "TableService";

    public string ConnectionString { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;

    public static bool Validate(TableServiceOptions options) =>
        !string.IsNullOrEmpty(options.ConnectionString) || !string.IsNullOrEmpty(options.Endpoint);
}
