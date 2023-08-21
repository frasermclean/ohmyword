using OhMyWord.Integrations.Options;

namespace OhMyWord.Integrations.Tests.Options;

public class TableServiceOptionsTests
{
    [Theory]
    [InlineData("connectionString", "", true)]
    [InlineData("", "endpoint", true)]
    [InlineData("", "", false)]
    public void TableServiceOptions_Validation_Should_Return_ExpectedResult(string connectionString,
        string endpoint, bool expectedResult)
    {
        // arrange
        var options = new TableServiceOptions { ConnectionString = connectionString, Endpoint = endpoint, };

        // act
        var isValid = TableServiceOptions.Validate(options);

        // assert
        isValid.Should().Be(expectedResult);
    }
}
