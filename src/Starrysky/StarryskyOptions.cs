namespace Starrysky;

internal sealed class StarryskyOptions
{
    public const string SectionName = Constants.ApplicationName;

    public string? GitHubToken { get; init; }

    public string? BlueskyHandle { get; init; }

    public string? BlueskyPassword { get; init; }
}
