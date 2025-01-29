using LibGit2Sharp;

namespace Starrysky.Services;

public interface IGitRepositoryFactory
{
    IRepository GetRepository(string repositoryRoot);
}
