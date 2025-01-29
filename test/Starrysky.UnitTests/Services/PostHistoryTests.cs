using System.IO.Abstractions;
using System.Text.Json;
using Starrysky.Services;

namespace Starrysky.UnitTests.Services;

public class PostHistoryTests
{
    [Fact]
    public async Task GetPostedIds_WithoutHistory_ReturnsEmpty()
    {
        // Arrange
        var gitRepository = Fakes.CreateGitRepository();
        var fileSystem = Substitute.For<IFileSystem>();
        var history = new PostHistory(gitRepository, fileSystem);

        // Act
        var ids = await history.GetPostedIds();

        // Assert
        ids.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetPostedIds_WithHistory_ReturnsIds()
    {
        // Arrange
        var gitRepository = Fakes.CreateGitRepository();
        long[] postIds = [1, 2, 3];
        var historyContent = postIds.Select(x => new { Id = x }).ToArray();
        var json = JsonSerializer.Serialize(historyContent);
        var historyFile = Substitute.For<IFile>();
        var path = Path.Combine(gitRepository.GetRepositoryRoot(), Constants.HistoryFileName);
        historyFile.Exists(path).Returns(true);
        historyFile.ReadAllTextAsync(path).Returns(Task.FromResult(json));
        var fileSystem = Substitute.For<IFileSystem>();
        fileSystem.File.Returns(historyFile);
        var history = new PostHistory(gitRepository, fileSystem);

        // Act
        var ids = await history.GetPostedIds();

        // Assert
        ids.ShouldBe(postIds);
    }

    [Fact]
    public async Task AddRepository_WithoutHistory_AddsRepository()
    {
        // Arrange
        var gitRepository = Fakes.CreateGitRepository();
        var historyFile = Substitute.For<IFile>();
        var path = Path.Combine(gitRepository.GetRepositoryRoot(), Constants.HistoryFileName);
        historyFile.Exists(path).Returns(false);
        var fileSystem = Substitute.For<IFileSystem>();
        fileSystem.File.Returns(historyFile);
        var history = new PostHistory(gitRepository, fileSystem);
        var repository = Fakes.CreateRepository();

        // Act
        await history.AddPosted(repository);

        // Assert
        await historyFile.Received().WriteAllTextAsync(path, Arg.Any<string>());
    }
}
