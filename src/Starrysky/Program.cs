using System.IO.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using Starrysky;
using Starrysky.Infrastructure;
using Starrysky.Services;

AnsiConsole.MarkupLine($"[green]{Constants.ApplicationName}[/] [yellow]{Constants.Version}[/]");

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()
    .Build();

var services = new ServiceCollection()
    .Configure<StarryskyOptions>(configuration.GetSection(StarryskyOptions.SectionName))
    .AddSingleton<IGitHubClientFactory, GitHubClientFactory>()
    .AddSingleton<IFileSystem, FileSystem>()
    .AddSingleton<GitHubQuery>() // Decorated with CachedGitHubQuery
    .AddSingleton<IGitHubQuery, CachedGitHubQuery>()
    .AddSingleton<IPostHistory, PostHistory>()
    .AddSingleton<IPostBuilder, PostBuilder>()
    .AddSingleton<IBlueskyPoster, BlueskyPoster>()
    .AddSingleton<IGitRepositoryFactory, GitRepositoryFactory>()
    .AddSingleton<IGitRepository, GitRepository>();
var registrar = new TypeRegistrar(services);

var app = new CommandApp<StarryskyCommand>(registrar);
app.Configure(config => config.Settings.ApplicationName = Constants.ApplicationName);
await app.RunAsync(args);
