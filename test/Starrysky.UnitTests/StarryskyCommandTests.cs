using Microsoft.Extensions.Options;
using Octokit;
using Spectre.Console.Cli;
using Starrysky.Services;

namespace Starrysky.UnitTests;

public class StarryskyCommandTests
{
    [Theory]
    [PairwiseData]
    public void ShouldValidateSettings(
        [CombinatorialValues("", " ", null, "handle")] string handle,
        [CombinatorialValues("", " ", null, "password")] string password,
        [CombinatorialValues("", " ", null, "token")] string token)
    {
        // Arrange
        var options = Options.Create(new StarryskyOptions());
        var settings = new StarryskyCommand.Settings(options)
        {
            BlueskyHandle = handle,
            BlueskyPassword = password,
            GitHubToken = token
        };

        // Act
        var result = settings.Validate();

        // Assert
        var expected = !string.IsNullOrWhiteSpace(handle)
                       && !string.IsNullOrWhiteSpace(password)
                       && !string.IsNullOrWhiteSpace(token);
        result.Successful.ShouldBe(expected);
    }

    [Fact]
    public void DryRunDefaultsToFalse()
    {
        // Arrange
        var options = Options.Create(new StarryskyOptions());

        // Act
        var settings = new StarryskyCommand.Settings(options);

        // Assert
        settings.DryRun.ShouldBeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DryRun(bool expected)
    {
        // Arrange
        var options = Options.Create(new StarryskyOptions());

        // Act
        var settings = new StarryskyCommand.Settings(options) { DryRun = expected };

        // Assert
        settings.DryRun.ShouldBe(expected);
    }

    [Fact]
    public void HeaderDefaultsToNull()
    {
        // Arrange
        var options = Options.Create(new StarryskyOptions());

        // Act
        var settings = new StarryskyCommand.Settings(options);

        // Assert
        settings.Header.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Header(string? expected)
    {
        // Arrange
        var options = Options.Create(new StarryskyOptions());

        // Act
        var settings = new StarryskyCommand.Settings(options) { Header = expected };

        // Assert
        settings.Header.ShouldBe(expected);
    }

    [Fact]
    public async Task ShouldReturnZeroExitCodeOnSuccess()
    {
        // Arrange
        var gitHubQuery = Substitute.For<IGitHubQuery>();
        var postHistory = Substitute.For<IPostHistory>();
        var options = Options.Create(new StarryskyOptions());
        var settings = new StarryskyCommand.Settings(options);
        var postBuilder = new PostBuilder(settings);
        var blueskyPoster = Substitute.For<IBlueskyPoster>();
        var gitRepository = Substitute.For<IGitRepository>();
        var command = new StarryskyCommand(gitHubQuery, postHistory, postBuilder, blueskyPoster, gitRepository);
        var commandContext = new CommandContext([], Substitute.For<IRemainingArguments>(), "__default_command", null);

        IReadOnlyList<Repository> repositories = [Fakes.CreateRepository()];
        gitHubQuery.GetRepositories().Returns(Task.FromResult(repositories));

        postHistory.GetPostedIds().Returns(Task.FromResult(new HashSet<long>()));

        // Act
        var exitCode = await command.ExecuteAsync(commandContext, settings);

        // Assert
        exitCode.ShouldBe(0);
    }
}
