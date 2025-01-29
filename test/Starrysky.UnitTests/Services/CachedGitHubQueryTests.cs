using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Octokit;
using Octokit.Internal;
using Starrysky.Services;

namespace Starrysky.UnitTests.Services;

public class CachedGitHubQueryTests
{
    [Fact]
    public async Task GetRepositories_NoCaching_ReturnsFromClient()
    {
        // Arrange
        var query = BuildQuery(null);

        // Act
        var repos = await query.GetRepositories();

        // Assert
        repos.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetRepositories_Caching_ReadsFromFile()
    {
        // Arrange
        var path = Path.Combine(Environment.CurrentDirectory, Constants.RepositoryCacheFileName);
        var repository = Fakes.CreateRepository();
        var serializer = new SimpleJsonSerializer();
        var json = serializer.Serialize(new[] { repository });
        var cacheFile = Substitute.For<IFile>();
        cacheFile.Exists(path).Returns(true);
        cacheFile.ReadAllTextAsync(path).Returns(Task.FromResult(json));

        var query = BuildQuery(cacheFile);

        // Act
        var repos = await query.GetRepositories();

        // Assert
        repos.Count.ShouldBe(1);
        repos[0].ShouldBeEquivalentTo(repository);
    }

    [Fact]
    public async Task GetRepositories_Caching_WritesToFile()
    {
        // Arrange
        string? json = null;
        var path = Path.Combine(Environment.CurrentDirectory, Constants.RepositoryCacheFileName);
        var cacheFile = Substitute.For<IFile>();
        cacheFile.When(x => x.WriteAllTextAsync(path, Arg.Any<string?>()))
            .Do(x => json = x.ArgAt<string?>(1));

        var query = BuildQuery(cacheFile);

        // Act
        var repos = await query.GetRepositories();

        // Assert
        repos.Count.ShouldBe(1);
        var actual = JsonSerializer.Deserialize<JsonArray>(json!);
        actual!.Count.ShouldBe(1);
    }

    private static CachedGitHubQuery BuildQuery(IFile? file)
    {
        var caching = file is not null;
        var client = Substitute.For<IGitHubClient>();
        IReadOnlyList<Repository> expected = [Fakes.CreateRepository()];
        client.Activity.Starring.GetAllForCurrent(Arg.Any<ApiOptions>()).Returns(Task.FromResult(expected));

        var factory = Substitute.For<IGitHubClientFactory>();
        factory.GetClient().Returns(client);

        var gitHubQuery = new GitHubQuery(factory);
        var gitRepository = Fakes.CreateGitRepository();
        var options = Options.Create(new StarryskyOptions());
        var fileSystem = Substitute.For<IFileSystem>();
        if (caching)
        {
            fileSystem.File.Returns(file);
        }
        var settings = new StarryskyCommand.Settings(options) { Caching = caching };

        return new CachedGitHubQuery(gitHubQuery, fileSystem, gitRepository, settings);
    }
}
