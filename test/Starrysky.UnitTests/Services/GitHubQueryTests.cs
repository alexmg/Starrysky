using Octokit;
using Starrysky.Services;

namespace Starrysky.UnitTests.Services;

public class GitHubQueryTests
{
    [Fact]
    public async Task GetRepositories_StarsAvailable_ReturnsStarredRepositories()
    {
        // Arrange
        var client = Substitute.For<IGitHubClient>();
        IReadOnlyList<Repository> expected = [Fakes.CreateRepository()];
        client.Activity.Starring.GetAllForCurrent(Arg.Any<ApiOptions>())
            .Returns(Task.FromResult(expected));

        var factory = Substitute.For<IGitHubClientFactory>();
        factory.GetClient().Returns(client);

        var gitHubQuery = new GitHubQuery(factory);

        // Act
        var repositories = await gitHubQuery.GetRepositories();

        // Assert
        repositories.ShouldBeEquivalentTo(expected);
    }
}
