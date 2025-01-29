using idunno.Bluesky;
using Octokit;

namespace Starrysky.Services;

internal interface IPostBuilder
{
    Post Build(Repository repository);
}
