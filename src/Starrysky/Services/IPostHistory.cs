using Octokit;

namespace Starrysky.Services;

internal interface IPostHistory
{
    Task<HashSet<long>> GetPostedIds();

    Task AddPosted(Repository repository);
}
