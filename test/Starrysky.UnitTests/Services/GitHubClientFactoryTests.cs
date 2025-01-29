using Microsoft.Extensions.Options;
using Octokit;
using Starrysky.Services;

namespace Starrysky.UnitTests.Services;

public class GitHubClientFactoryTests
{
    [Fact]
    public void GetClient_ValidConfiguration_ReturnsInstance()
    {
        // Arrange
        const string? gitHubToken = "token";
        var options = Options.Create(new StarryskyOptions { BlueskyHandle = "handle" });
        var settings = new StarryskyCommand.Settings(options) { GitHubToken = gitHubToken };
        var factory = new GitHubClientFactory(settings);

        // Act
        var client = factory.GetClient();

        // Assert
        client.ShouldNotBeNull();
        var connection = client.Connection;
        var credentials = connection.Credentials;
        var productVersion = $"{Constants.ApplicationName}-{options.Value.BlueskyHandle}";
        credentials.AuthenticationType.ShouldBe(AuthenticationType.Oauth);
        credentials.Password.ShouldBe(gitHubToken);
        connection.ShouldBeOfType<Connection>().UserAgent.ShouldStartWith(productVersion);
    }
}
