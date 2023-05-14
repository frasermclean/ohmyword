using OhMyWord.Infrastructure.Options;

namespace Infrastructure.Tests.Options;

public class CosmosDbOptionsTests
{
    [Theory]
    [InlineData("connectionString", "", "databaseId", new[] { "containerId" }, true)]
    [InlineData("", "accountEndpoint", "databaseId", new[] { "containerId" }, true)]
    [InlineData("", "", "databaseId", new[] { "containerId" }, false)]
    [InlineData("connectionString", "", "", null, false)]
    public void CosmosDbOptions_Validation_Should_Return_ExpectedResult(string connectionString, string accountEndpoint,
        string databaseId, string[] containerIds, bool expectedResult)
    {
        // arrange
        var options = new CosmosDbOptions
        {
            ApplicationName = "Test Application",
            ConnectionString = connectionString,
            AccountEndpoint = accountEndpoint,
            DatabaseId = databaseId,
            ContainerIds = containerIds,
        };

        // act
        var isValid = CosmosDbOptions.Validate(options);

        // assert
        isValid.Should().Be(expectedResult);
        options.ApplicationName.Should().Be("Test Application");
    }
}
