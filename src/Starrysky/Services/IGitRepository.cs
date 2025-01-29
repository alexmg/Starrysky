using Octokit;

namespace Starrysky.Services;

internal interface IGitRepository
{
    string GetRepositoryRoot();

    void Commit(Repository repository);

    Task Push();
}
