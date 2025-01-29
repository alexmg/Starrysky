using System.Diagnostics.CodeAnalysis;

namespace Starrysky;

[ExcludeFromCodeCoverage]
internal static class Constants
{
    internal const string HistoryFileName = "history.json";

    internal const string RepositoryCacheFileName = "repos.json";

    internal const string RepositoryUrl = ThisAssembly.RepositoryUrl;

    internal const string ApplicationName = ThisAssembly.AssemblyName;

    internal static readonly string Version = new Version(ThisAssembly.Version).ToString(3);
}
