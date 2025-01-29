namespace Starrysky.UnitTests;

public class StarryskyOptionsTests
{
    [Fact]
    public void Constructor_Default_AllPropertiesNull()
    {
        // Arrange/Act
        var options = new StarryskyOptions();

        // Assert
        options.ShouldNotBeNull();
        options.BlueskyHandle.ShouldBeNull();
        options.BlueskyPassword.ShouldBeNull();
        options.GitHubToken.ShouldBeNull();
    }

    [Fact]
    public void Constructor_Initializer_SetsProperties()
    {
        // Arrange
        const string handle = "handle";
        const string password = "password";
        const string token = "token";

        // Act
        var options = new StarryskyOptions
        {
            BlueskyHandle = handle,
            BlueskyPassword = password,
            GitHubToken = token
        };

        // Assert
        options.ShouldNotBeNull();
        options.BlueskyHandle.ShouldBe(handle);
        options.BlueskyPassword.ShouldBe(password);
        options.GitHubToken.ShouldBe(token);
    }
}
