using idunno.Bluesky;
using idunno.Bluesky.Embed;
using idunno.Bluesky.RichText;
using Octokit;

namespace Starrysky.Services;

internal sealed class PostBuilder : IPostBuilder
{
    private readonly StarryskyCommand.Settings _settings;

    public PostBuilder(StarryskyCommand.Settings settings) => _settings = settings;

    public Post Build(Repository repository)
    {
        var builder = new idunno.Bluesky.PostBuilder();

        builder.Append(_settings.Header ?? "Today's random GitHub \u2B50!");
        builder.Append(Environment.NewLine);
        builder.Append(Environment.NewLine);
        var link = new Link(repository.HtmlUrl, repository.GetName());
        builder.Append(link);

        if (_settings.Footer)
        {
            builder.Append(Environment.NewLine);
            builder.Append(Environment.NewLine);
            builder.Append("Posted using ");
            builder.Append(new Link(Constants.RepositoryUrl, Constants.ApplicationName));
        }

        var external = new EmbeddedExternal(
            repository.HtmlUrl,
            $"GitHub - {repository.GetName()}",
            repository.Description);
        builder.EmbedRecord(external);

        return builder.ToPost();
    }
}
