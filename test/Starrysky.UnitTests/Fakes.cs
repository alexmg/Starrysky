using Octokit;
using Octokit.Internal;
using Starrysky.Services;

namespace Starrysky.UnitTests;

public static class Fakes
{
    public static Repository CreateRepository(string name = "cool-lib", string owner = "dudes") =>
        new SimpleJsonSerializer()
            .Deserialize<Repository>(
                //language=JSON
                $$"""
                  {
                    "id": 123,
                    "name": "{{name}}",
                    "description": "{{name}} is a cool repo",
                    "html_url": "https://github.com/{{owner}}/{{name}}",
                    "owner": {
                      "login": "{{owner}}"
                    }
                  }
                  """);

    public static User CreateUser(string login = "username") =>
        new SimpleJsonSerializer()
            .Deserialize<User>(
                //language=JSON
                $$"""
                  {
                    "login": "{{login}}"
                  }
                  """);

    internal static IGitRepository CreateGitRepository()
    {
        var gitRepository = Substitute.For<IGitRepository>();
        gitRepository.GetRepositoryRoot().Returns(Environment.CurrentDirectory);
        return gitRepository;
    }
}
