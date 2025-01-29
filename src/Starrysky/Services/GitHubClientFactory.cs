using Octokit;

namespace Starrysky.Services;

internal class GitHubClientFactory : IGitHubClientFactory
{
    private readonly StarryskyCommand.Settings _settings;

    public GitHubClientFactory(StarryskyCommand.Settings settings) => _settings = settings;

    public IGitHubClient GetClient()
    {
        var product = $"{Constants.ApplicationName}-{_settings.BlueskyHandle}";

        return new GitHubClient(new ProductHeaderValue(product))
        {
            Credentials = new Credentials(_settings.GitHubToken)
        };
    }
}
