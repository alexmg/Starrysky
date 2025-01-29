using Octokit;

namespace Starrysky.Services;

internal interface IGitHubClientFactory
{
    IGitHubClient GetClient();
}
