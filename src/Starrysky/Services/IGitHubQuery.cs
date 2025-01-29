using Octokit;

namespace Starrysky.Services;

internal interface IGitHubQuery
{
    Task<IReadOnlyList<Repository>> GetRepositories();
}
