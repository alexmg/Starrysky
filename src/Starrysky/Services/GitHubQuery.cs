using Octokit;

namespace Starrysky.Services;

internal sealed class GitHubQuery : IGitHubQuery
{
    private readonly IGitHubClientFactory _clientFactory;

    public GitHubQuery(IGitHubClientFactory clientFactory) => _clientFactory = clientFactory;

    public async Task<IReadOnlyList<Repository>> GetRepositories()
    {
        var client = _clientFactory.GetClient();
        var options = new ApiOptions { PageSize = 100 };
        return await client.Activity.Starring.GetAllForCurrent(options);
    }
}
