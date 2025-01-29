using LibGit2Sharp;

namespace Starrysky.Services;

public class GitRepositoryFactory : IGitRepositoryFactory
{
    public IRepository GetRepository(string repositoryRoot) => new Repository(repositoryRoot);
}
