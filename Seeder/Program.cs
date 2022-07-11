const string DefaultDatabaseName = "OhMyWord";
const string DefaultConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

Console.WriteLine("This program will attempt to populate an OhMyWord database.");

var databaseName = GetUserResponse($"Database name ({DefaultDatabaseName}): ", DefaultDatabaseName);
var connectionString = GetUserResponse($"Connection string (local emulator): ", DefaultConnectionString);

static string GetUserResponse(string prompt, string defaultResponse)
{
    Console.Write(prompt);
    var response = Console.ReadLine();
    return string.IsNullOrEmpty(response) ? defaultResponse : response;
}
