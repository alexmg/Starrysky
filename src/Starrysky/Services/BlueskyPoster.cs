using System.Diagnostics.CodeAnalysis;
using idunno.Bluesky;

namespace Starrysky.Services;

[ExcludeFromCodeCoverage]
internal sealed class BlueskyPoster : IBlueskyPoster
{
    private readonly StarryskyCommand.Settings _settings;

    public BlueskyPoster(StarryskyCommand.Settings settings) => _settings = settings;

    public async Task Post(Post post)
    {
        var handle = _settings.BlueskyHandle;
        var password = _settings.BlueskyPassword;

        using var agent = new BlueskyAgent();

        var loginResult = await agent.Login(handle!, password!);
        loginResult.EnsureSucceeded();

        var postResponse = await agent.Post(post);
        postResponse.EnsureSucceeded();
    }
}
