using OhMyWord.Infrastructure.Options;

namespace OhMyWord.Integrations.Tests.Options;

public class CosmosDbOptionsTests
{
    [Theory]
    [InlineData("connectionString", "", "databaseId", true)]
    [InlineData("", "accountEndpoint", "databaseId", true)]
    [InlineData("", "", "databaseId", false)]
    [InlineData("connectionString", "", "", false)]
    public void CosmosDbOptions_Validation_Should_Return_ExpectedResult(string connectionString, string accountEndpoint,
        string databaseId, bool expectedResult)
    {
        // arrange
        var options = new CosmosDbOptions
        {
            ApplicationName = "Test Application",
            ConnectionString = connectionString,
            AccountEndpoint = accountEndpoint,
            DatabaseId = databaseId
        };

        // act
        var isValid = CosmosDbOptions.Validate(options);

        // assert
        isValid.Should().Be(expectedResult);
        options.ApplicationName.Should().Be("Test Application");
    }
}
