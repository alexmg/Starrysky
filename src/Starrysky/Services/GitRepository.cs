using LibGit2Sharp;
using GitRepo = LibGit2Sharp.Repository;
using Repository = Octokit.Repository;

namespace Starrysky.Services;

internal sealed class GitRepository : IGitRepository
{
    private readonly IGitRepositoryFactory _factory;
    private readonly IGitHubClientFactory _clientFactory;
    private readonly StarryskyCommand.Settings _settings;
    private readonly string _repositoryRoot;

    public GitRepository(
        IGitRepositoryFactory factory,
        IGitHubClientFactory clientFactory,
        StarryskyCommand.Settings settings)
    {
        _factory = factory;
        _clientFactory = clientFactory;
        _settings = settings;
        _repositoryRoot = FindRepositoryRoot();
    }

    public string GetRepositoryRoot() => _repositoryRoot;

    public void Commit(Repository repository)
    {
        using var gitRepo = _factory.GetRepository(_repositoryRoot);

        var signature = gitRepo.Config.BuildSignature(DateTimeOffset.Now)
                        ?? throw new InvalidOperationException(
                            "The author information was not found in the Git repository configuration");

        gitRepo.Index.Add(Constants.HistoryFileName);

        var message = $"Posted starred GitHub repository '{repository.GetName()}' to Bluesky";
        gitRepo.Commit(message, signature, signature);
    }

    public async Task Push()
    {
        using var gitRepo = _factory.GetRepository(_repositoryRoot);

        var client = _clientFactory.GetClient();
        var user = await client.User.Current();

        var pushOptions = new PushOptions
        {
            CredentialsProvider = (_, _, _) =>
                new UsernamePasswordCredentials
                {
                    Username = user.Login,
                    Password = _settings.GitHubToken
                }
        };
        gitRepo.Network.Push(gitRepo.Head, pushOptions);
    }

    internal static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(Environment.CurrentDirectory);
        while (directory != null)
        {
            if (GitRepo.IsValid(directory.FullName))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }

        throw new InvalidOperationException("No Git repository was found");
    }
}
