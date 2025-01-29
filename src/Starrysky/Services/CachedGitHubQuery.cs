using System.IO.Abstractions;
using Octokit;
using Octokit.Internal;

namespace Starrysky.Services;

internal sealed class CachedGitHubQuery : IGitHubQuery
{
    private readonly GitHubQuery _query;
    private readonly IFileSystem _fileSystem;
    private readonly StarryskyCommand.Settings _settings;
    private readonly IGitRepository _gitRepository;

    public CachedGitHubQuery(
        GitHubQuery query,
        IFileSystem fileSystem,
        IGitRepository gitRepository,
        StarryskyCommand.Settings settings)
    {
        _query = query;
        _fileSystem = fileSystem;
        _settings = settings;
        _gitRepository = gitRepository;
    }

    public async Task<IReadOnlyList<Repository>> GetRepositories()
    {
        if (!_settings.Caching)
        {
            return await _query.GetRepositories();
        }

        var serializer = new SimpleJsonSerializer();
        var cacheFilePath = Path.Combine(_gitRepository.GetRepositoryRoot(), Constants.RepositoryCacheFileName);

        if (_fileSystem.File.Exists(cacheFilePath))
        {
            var json = await _fileSystem.File.ReadAllTextAsync(cacheFilePath);
            return serializer.Deserialize<List<Repository>>(json);
        }
        else
        {
            var repositories = await _query.GetRepositories();
            var json = serializer.Serialize(repositories);
            await _fileSystem.File.WriteAllTextAsync(cacheFilePath, json);
            return repositories;
        }
    }
}
