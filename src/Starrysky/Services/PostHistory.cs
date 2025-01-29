using System.IO.Abstractions;
using System.Text.Json;
using JetBrains.Annotations;
using Octokit;

namespace Starrysky.Services;

internal sealed class PostHistory : IPostHistory
{
    private readonly IFileSystem _fileSystem;
    private readonly string _historyFilePath;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerOptions.Default)
    {
        WriteIndented = true
    };

    public PostHistory(IGitRepository gitRepository, IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        _historyFilePath = Path.Combine(gitRepository.GetRepositoryRoot(), Constants.HistoryFileName);
    }

    public async Task<HashSet<long>> GetPostedIds()
    {
        var history = await GetHistoryData();
        return history.Select(x => x.Id).ToHashSet();
    }

    public async Task AddPosted(Repository repository)
    {
        var history = await GetHistoryData();
        history.Add(new HistoryEntry(repository.Id, repository.GetName(), DateTimeOffset.Now));
        var data = JsonSerializer.Serialize(history, SerializerOptions);
        await _fileSystem.File.WriteAllTextAsync(_historyFilePath, data);
    }

    private async Task<List<HistoryEntry>> GetHistoryData()
    {
        if (!_fileSystem.File.Exists(_historyFilePath))
        {
            return [];
        }

        var json = await _fileSystem.File.ReadAllTextAsync(_historyFilePath);
        var history = JsonSerializer.Deserialize<List<HistoryEntry>>(json, SerializerOptions) ?? [];
        return history;
    }

    private sealed record HistoryEntry(long Id, [UsedImplicitly] string Name, DateTimeOffset Timestamp);
}
