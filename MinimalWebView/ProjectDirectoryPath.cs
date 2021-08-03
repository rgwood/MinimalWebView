#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

/// <summary>
/// Helper to get the root path of the MinimalWebView project for hot reload scenarios
/// https://stackoverflow.com/a/66285728/854694
/// </summary>
internal static class ProjectDirectoryPath
{
    private const string myFileName = nameof(ProjectDirectoryPath) + ".cs";
    private static string? lazyValue;

    public static string Value => lazyValue ??= CalculateProjectDirectoryPath();

    private static string CalculateProjectDirectoryPath()
    {
        string pathName = GetSourceFilePathName();
        Trace.Assert(pathName.EndsWith(myFileName, StringComparison.Ordinal));
        return pathName.Substring(0, pathName.Length - myFileName.Length);
    }

    private static string GetSourceFilePathName([CallerFilePath] string? callerFilePath = null)
        => callerFilePath ?? "";
}
#nullable restore