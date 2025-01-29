using System.ComponentModel;
using Dumpify;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;
using Starrysky.Services;

namespace Starrysky;

[UsedImplicitly]
internal sealed class StarryskyCommand : AsyncCommand<StarryskyCommand.Settings>
{
    private readonly IGitHubQuery _gitHubQuery;
    private readonly IPostHistory _postHistory;
    private readonly IPostBuilder _postBuilder;
    private readonly IBlueskyPoster _blueskyPoster;
    private readonly IGitRepository _gitRepository;

    public StarryskyCommand(
        IGitHubQuery gitHubQuery,
        IPostHistory postHistory,
        IPostBuilder postBuilder,
        IBlueskyPoster blueskyPoster,
        IGitRepository gitRepository)
    {
        _gitHubQuery = gitHubQuery;
        _postHistory = postHistory;
        _postBuilder = postBuilder;
        _blueskyPoster = blueskyPoster;
        _gitRepository = gitRepository;
    }

    [UsedImplicitly]
    public sealed class Settings : CommandSettings
    {
        public Settings(IOptions<StarryskyOptions> options)
        {
            var overrides = options.Value;

            if (GitHubToken is null && overrides.GitHubToken is not null)
            {
                GitHubToken = overrides.GitHubToken;
            }

            if (BlueskyHandle is null && overrides.BlueskyHandle is not null)
            {
                BlueskyHandle = overrides.BlueskyHandle;
            }

            if (BlueskyPassword is null && overrides.BlueskyPassword is not null)
            {
                BlueskyPassword = overrides.BlueskyPassword;
            }
        }

        [Description("Token for access to the GitHub API")]
        [CommandOption("-t|--token")]
        [DefaultValue(null)]
        public string? GitHubToken { get; [UsedImplicitly] init; }

        [Description("Handle of the Bluesky account")]
        [CommandOption("--handle")]
        [DefaultValue(null)]
        public string? BlueskyHandle { get; [UsedImplicitly] init; }

        [Description("Password for the Bluesky account")]
        [CommandOption("-p|--password")]
        [DefaultValue(null)]
        public string? BlueskyPassword { get; [UsedImplicitly] init; }

        [Description("Prints the post to the console without posting to Bluesky or saving history")]
        [CommandOption("-d|--dry-run")]
        [DefaultValue(false)]
        public bool DryRun { get; [UsedImplicitly] init; }

        [Description("Enables caching of the starred GitHub repositories retrieved from the GitHub API (useful during development and testing)")]
        [CommandOption("-c|--caching")]
        [DefaultValue(false)]
        public bool Caching { get; [UsedImplicitly] init; }

        [Description("Include a footer in the post with a link to this project")]
        [CommandOption("-f|--footer")]
        [DefaultValue(true)]
        public bool Footer { get; [UsedImplicitly] init; }

        [Description("A custom header to use for the post")]
        [CommandOption("--header")]
        [DefaultValue(null)]
        public string? Header { get; [UsedImplicitly] init; }

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(BlueskyHandle))
            {
                return ValidationResult.Error(
                    BuildErrorMessage("Bluesky handle", nameof(StarryskyOptions.BlueskyHandle)));
            }

            if (string.IsNullOrWhiteSpace(BlueskyPassword))
            {
                return ValidationResult.Error(
                    BuildErrorMessage("Bluesky password", nameof(StarryskyOptions.BlueskyPassword)));
            }

            if (string.IsNullOrWhiteSpace(GitHubToken))
            {
                return ValidationResult.Error(
                    BuildErrorMessage("GitHub token", nameof(StarryskyOptions.GitHubToken)));
            }

            return ValidationResult.Success();

            static string BuildErrorMessage(string prefix, string optionName)
            {
                optionName = $"{Constants.ApplicationName}__{optionName}";
                return $"{prefix} must be provided via command line option or environment variable '{optionName}'";
            }
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            AnsiConsole.WriteLine($"Getting starred repositories from GitHub{(settings.Caching ? " (with caching)" : string.Empty)}");
            var repositories = await _gitHubQuery.GetRepositories();
            AnsiConsole.MarkupLine($"Found [green]{repositories.Count}[/] total starred repositories in GitHub");

            var postedIds = await _postHistory.GetPostedIds();
            var availableRepositories = repositories.Where(x => !postedIds.Contains(x.Id)).ToList();

            if (availableRepositories.Count == 0)
            {
                AnsiConsole.MarkupLine("No starred repositories were found that have not already been posted");
                return 0;
            }

            AnsiConsole.MarkupLine($"There are [green]{availableRepositories.Count}[/] starred repositories that have not been posted");

            var repository = availableRepositories[Random.Shared.Next(0, availableRepositories.Count - 1)];
            AnsiConsole.MarkupLine($"Selected random repository [green]{repository.GetName()}[/] to post");

            AnsiConsole.WriteLine("Building the Bluesky post with the repository details");
            var record = _postBuilder.Build(repository);
            record.Text.Dump("Post");

            if (settings.DryRun)
            {
                AnsiConsole.WriteLine("Dry run enabled, skipping post to Bluesky");
                return 0;
            }

            AnsiConsole.WriteLine("Posting starred repository to Bluesky");
            await _blueskyPoster.Post(record);
            AnsiConsole.WriteLine("Post has been successfully created");

            AnsiConsole.WriteLine("Adding starred repository to post history");
            await _postHistory.AddPosted(repository);
            AnsiConsole.WriteLine("Repository has been added to post history");

            AnsiConsole.WriteLine("Commiting post history to Git repository");
            _gitRepository.Commit(repository);
            AnsiConsole.WriteLine("Post history has been committed to Git repository");

            AnsiConsole.WriteLine("Pushing changes to remote");
            await _gitRepository.Push();
            AnsiConsole.WriteLine("Pushed changes to remote");

            AnsiConsole.WriteLine("✔️ Command completed successfully");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"❌ Failed: [red]{ex.Message}[/]");
            return 1;
        }
    }
}
