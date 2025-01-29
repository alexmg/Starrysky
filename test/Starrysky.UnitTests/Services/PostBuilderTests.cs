using idunno.Bluesky.Embed;
using idunno.Bluesky.RichText;
using Microsoft.Extensions.Options;
using Octokit;
using Starrysky.Services;

namespace Starrysky.UnitTests.Services;

public class PostBuilderTests
{
    [Fact]
    public void Build_WithFooter()
    {
        var repository = Fakes.CreateRepository();
        var text =
            $"""
             Today's random GitHub ⭐!

             {repository.GetName()}
             
             Posted using Starrysky
             """;
        Uri[] facetUris = [new(repository.HtmlUrl), new(Constants.RepositoryUrl)];
        AssertBuild(repository, text, facetUris, true);
    }

    [Fact]
    public void Build_WithoutFooter()
    {
        var repository = Fakes.CreateRepository();
        var text =
            $"""
             Today's random GitHub ⭐!

             {repository.GetName()}
             """;
        Uri[] facetUris = [new(repository.HtmlUrl)];
        AssertBuild(repository, text, facetUris, false);
    }

    private static void AssertBuild(Repository repository, string text, Uri[] facetUris, bool footer)
    {
        var options = Options.Create(new StarryskyOptions());
        var settings = new StarryskyCommand.Settings(options) { Footer = footer };
        var builder = new PostBuilder(settings);

        var post = builder.Build(repository);

        post.Text.ShouldBe(text);
        var embed = (EmbeddedExternal)post.EmbeddedRecord!;
        embed.External.Title.ShouldBe($"GitHub - {repository.GetName()}");
        embed.External.Uri.ShouldBe(new Uri(repository.HtmlUrl));
        embed.External.Description.ShouldBe(repository.Description);
        post.Facets!.Count.ShouldBe(facetUris.Length);
        post.Facets.ShouldContain(x => facetUris.Any(f => ((LinkFacetFeature)x.Features[0]).Uri == f));
    }
}
