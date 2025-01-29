using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.Extensions.Options;
using Octokit;
using Starrysky.Services;
using Credentials = LibGit2Sharp.Credentials;
using Repository = LibGit2Sharp.Repository;
using Signature = LibGit2Sharp.Signature;

namespace Starrysky.UnitTests.Services;

public class GitRepositoryTests
{
    [Fact]
    public void GetRepositoryRoot()
    {
        // Arrange
        var options = Options.Create(new StarryskyOptions());
        var settings = new StarryskyCommand.Settings(options);
        var repoFactory = Substitute.For<IGitRepositoryFactory>();
        var clientFactory = Substitute.For<IGitHubClientFactory>();
        var gitRepository = new GitRepository(repoFactory, clientFactory, settings);

        // Act
        var root = gitRepository.GetRepositoryRoot();

        // Assert
        Repository.IsValid(root).ShouldBeTrue();
    }

    [Fact]
    public void Commit()
    {
        // Arrange
        var repoFactory = Substitute.For<IGitRepositoryFactory>();
        var gitRepo = Substitute.For<IRepository>();
        var clientFactory = Substitute.For<IGitHubClientFactory>();
        var options = Options.Create(new StarryskyOptions());
        var settings = new StarryskyCommand.Settings(options);
        var gitRepository = new GitRepository(repoFactory, clientFactory, settings);
        repoFactory.GetRepository(gitRepository.GetRepositoryRoot()).Returns(gitRepo);
        var repository = Fakes.CreateRepository();
        var config = Substitute.For<Configuration>();
        var signature = new Signature("foo", "foo@email.com", DateTimeOffset.UtcNow);
        config.BuildSignature(Arg.Any<DateTimeOffset>()).Returns(signature);
        gitRepo.Config.Returns(config);

        // Act
        gitRepository.Commit(repository);

        // Assert
        gitRepo.Received().Index.Add(Constants.HistoryFileName);
        var message = $"Posted starred GitHub repository '{repository.GetName()}' to Bluesky";
        gitRepo.Received().Commit(message, signature, signature, Arg.Any<CommitOptions>());
    }

    [Fact]
    public async Task Push()
    {
        // Arrange
        var repoFactory = Substitute.For<IGitRepositoryFactory>();
        var gitRepo = Substitute.For<IRepository>();
        var clientFactory = Substitute.For<IGitHubClientFactory>();
        var client = Substitute.For<IGitHubClient>();
        clientFactory.GetClient().Returns(client);
        var user = Fakes.CreateUser();
        client.User.Current().Returns(Task.FromResult(user));
        var options = Options.Create(new StarryskyOptions { GitHubToken = "token" });
        var settings = new StarryskyCommand.Settings(options);
        var gitRepository = new GitRepository(repoFactory, clientFactory, settings);
        repoFactory.GetRepository(gitRepository.GetRepositoryRoot()).Returns(gitRepo);

        CredentialsHandler? credentialsProvider = null;
        gitRepo.Network.Push(gitRepo.Head, Arg.Do<PushOptions>(x => credentialsProvider = x.CredentialsProvider));

        // Act
        await gitRepository.Push();

        // Assert
        gitRepo.Received().Network.Push(gitRepo.Head, Arg.Is<PushOptions>(x => x.CredentialsProvider == credentialsProvider));
        const SupportedCredentialTypes types = SupportedCredentialTypes.Default | SupportedCredentialTypes.UsernamePassword;
        var credentials = (UsernamePasswordCredentials)credentialsProvider!(null, null, types);
        credentials.Username.ShouldBe(user.Login);
        credentials.Password.ShouldBe(settings.GitHubToken);
    }
}
