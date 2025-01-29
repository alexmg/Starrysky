using Octokit;

namespace Starrysky.Services;

internal static class RepositoryExtensions
{
    public static string GetName(this Repository repository) => $"{repository.Owner.Login}/{repository.Name}";
}
