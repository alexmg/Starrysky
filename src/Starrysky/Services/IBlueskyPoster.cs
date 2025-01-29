using idunno.Bluesky;

namespace Starrysky.Services;

internal interface IBlueskyPoster
{
    Task Post(Post post);
}
