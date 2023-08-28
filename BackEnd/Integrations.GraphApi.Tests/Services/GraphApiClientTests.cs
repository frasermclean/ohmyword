using OhMyWord.Core.Services;
using OhMyWord.Integrations.GraphApi.Tests.Fixtures;

namespace OhMyWord.Integrations.GraphApi.Tests.Services;

[Trait("Category", "Integration")]
public class GraphApiClientTests : IClassFixture<GraphApiClientFixture>
{
    private readonly IGraphApiClient graphApiClient;

    public GraphApiClientTests(GraphApiClientFixture fixture)
    {
        graphApiClient = fixture.GraphApiClient;
    }

    [Theory]
    [InlineData("69a85c34-9566-40ba-b43c-ffcf664f717d", "Fraser", "McLean")]
    [InlineData("601cd7d5-cc99-4942-8ded-5c15c8a0032e", "Bob", "Jones")]
    public async Task GetUserByIdAsync_Should_Return_Expected_Result(string userId, string firstName, string lastName)
    {
        var userDetails = await graphApiClient.GetUserDetailsAsync(Guid.Parse(userId));

        // assert
        userDetails.Firstname.Should().Be(firstName);
        userDetails.Lastname.Should().Be(lastName);
        userDetails.DisplayName.Should().Be($"{firstName} {lastName}");
    }
}
