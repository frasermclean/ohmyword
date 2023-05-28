using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OhMyWord.Infrastructure.DependencyInjection;
using OhMyWord.Infrastructure.Services.GraphApi;

namespace Infrastructure.Tests.Services;

[Trait("Category", "Integration")]
public class GraphApiClientTests
{
    private readonly IGraphApiClient graphApiClient;

    public GraphApiClientTests()
    {
        var host = new HostBuilder()
            .UseEnvironment("Development")
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddJsonFile("appsettings.json", false);
                builder.AddUserSecrets<GraphApiClientTests>();
            })
            .ConfigureServices((context, collection) =>
            {
                collection.AddGraphApiClient(context.Configuration);
            })
            .Build();

        graphApiClient = host.Services.GetRequiredService<IGraphApiClient>();
    }

    [Theory]
    [InlineData("69a85c34-9566-40ba-b43c-ffcf664f717d", "Fraser", "McLean")]
    [InlineData("601cd7d5-cc99-4942-8ded-5c15c8a0032e", "Bob", "Jones")]
    public async Task GetUserByIdAsync_Should_Return_Expected_Result(string userId, string firstName, string lastName)
    {
        var user = await graphApiClient.GetUserByIdAsync(Guid.Parse(userId));

        // assert
        Assert.NotNull(user);
        user.GivenName.Should().Be(firstName);
        user.Surname.Should().Be(lastName);
    }
}
