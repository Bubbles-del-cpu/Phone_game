#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Builds the release players into "&lt;version&gt; builds/&lt;Platform&gt;" so
/// Tools/Codex/package_cross_platform_builds.py can zip them afterwards.
/// Every build first verifies the bundled gallery code so all platforms ship the same one.
/// </summary>
public static class NtsReleaseBuilder
{
    private const string ProductFileName = "NTS Honeymoon";

    private static string Version => PlayerSettings.bundleVersion;
    private static string ProjectRoot => Path.GetDirectoryName(Application.dataPath);
    private static string BuildsRoot => Path.Combine(ProjectRoot, Version + " builds");
    private static string LogPath => Path.Combine(BuildsRoot, "build-log.txt");

    [MenuItem("Tools/NTS/Release Builds/Pre-build Check")]
    public static void PreBuildCheck()
    {
        Update020EpisodeInstaller.VerifyGalleryCode();

        var scenes = EnabledScenes();
        if (scenes.Length == 0)
            throw new InvalidOperationException("No scenes are enabled in Build Settings.");

        Log($"Pre-build check passed: version {Version}, Android code {PlayerSettings.Android.bundleVersionCode}, scenes [{string.Join(", ", scenes)}].");
    }

    [MenuItem("Tools/NTS/Release Builds/Build Windows")]
    public static void BuildWindows() =>
        Build(BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone, "Windows", $"{ProductFileName} {Version}.exe");

    [MenuItem("Tools/NTS/Release Builds/Build Linux")]
    public static void BuildLinux() =>
        Build(BuildTarget.StandaloneLinux64, BuildTargetGroup.Standalone, "Linux", $"{ProductFileName} {Version}.x86_64");

    [MenuItem("Tools/NTS/Release Builds/Build macOS")]
    public static void BuildMac() =>
        Build(BuildTarget.StandaloneOSX, BuildTargetGroup.Standalone, "Mac", $"{ProductFileName} {Version}.app");

    [MenuItem("Tools/NTS/Release Builds/Build Android")]
    public static void BuildAndroid() =>
        Build(BuildTarget.Android, BuildTargetGroup.Android, "Android", $"{ProductFileName} {Version} Android.apk");

    /// <summary>
    /// Batch-mode entry point: builds whichever platform Unity was launched with via -buildTarget.
    /// Run one Unity process per platform so Addressables content is built for the right target
    /// (see Tools/Codex/RELEASE_BUILDS.md).
    /// </summary>
    public static void BuildActiveTarget()
    {
        switch (EditorUserBuildSettings.activeBuildTarget)
        {
            case BuildTarget.StandaloneWindows64: BuildWindows(); break;
            case BuildTarget.StandaloneLinux64: BuildLinux(); break;
            case BuildTarget.StandaloneOSX: BuildMac(); break;
            case BuildTarget.Android: BuildAndroid(); break;
            default: throw new InvalidOperationException("No release build for " + EditorUserBuildSettings.activeBuildTarget);
        }
    }

    private static void Build(BuildTarget target, BuildTargetGroup group, string folder, string fileName)
    {
        // The MCP bridge re-sends a menu command when a long build makes it time out;
        // ignore repeats so one request doesn't turn into a chain of identical builds.
        var lastBuildKey = "NtsReleaseBuilder.LastBuild." + target;
        var lastBuild = SessionState.GetString(lastBuildKey, string.Empty);
        if (DateTime.TryParse(lastBuild, out var lastBuildTime) && DateTime.Now - lastBuildTime < TimeSpan.FromMinutes(10))
        {
            Log($"Skipped repeated {target} build request (last build finished {lastBuildTime:HH:mm:ss}).");
            return;
        }

        // Addressables packages its bundles for the active editor target, so never let BuildPlayer switch platforms itself.
        if (EditorUserBuildSettings.activeBuildTarget != target)
        {
            if (Application.isBatchMode)
                ReleaseBuildPlatformGuard.RequireActiveTarget(target);

            EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
            Log($"Switched the editor to {target}. Wait for importing and compiling to finish, then run this build again.");
            return;
        }

        PreBuildCheck();

        var outputFolder = Path.Combine(BuildsRoot, folder);
        if (Directory.Exists(outputFolder))
            Directory.Delete(outputFolder, true);
        Directory.CreateDirectory(outputFolder);

        if (target == BuildTarget.Android)
            EditorUserBuildSettings.buildAppBundle = false;

        Log($"Building {target} -> {folder}/{fileName}");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = EnabledScenes(),
            locationPathName = Path.Combine(outputFolder, fileName),
            target = target,
            targetGroup = group,
            options = BuildOptions.None,
        });

        var summary = report.summary;
        Log($"{target}: {summary.result}, {summary.totalErrors} errors, {summary.totalSize / 1024f / 1024f:F1} MB, {summary.totalTime:hh\\:mm\\:ss}");
        if (summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException($"{target} build failed: {summary.result}");

        SessionState.SetString(lastBuildKey, DateTime.Now.ToString("o"));
    }

    private static string[] EnabledScenes() =>
        EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();

    private static void Log(string message)
    {
        Directory.CreateDirectory(BuildsRoot);
        File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
        Debug.Log("[NtsReleaseBuilder] " + message);
    }
}
#endif
