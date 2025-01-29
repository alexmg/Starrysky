using Starrysky.Services;

namespace Starrysky.UnitTests.Services;

public class GitRepositoryFactoryTests
{
    [Fact]
    public void GetRepository_ReturnsInstance()
    {
        // Arrange
        var factory = new GitRepositoryFactory();
        var repositoryRoot = GitRepository.FindRepositoryRoot();

        // Act
        var repository = factory.GetRepository(repositoryRoot);

        // Assert
        repository.Info.Path.ShouldStartWith(repositoryRoot);
    }
}
