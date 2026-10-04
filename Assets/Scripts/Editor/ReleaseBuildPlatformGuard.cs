#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;

// Addressables reads the active editor target before BuildPlayer switches targets.
// Fail early instead of silently shipping the previous platform's bundles.
public sealed class ReleaseBuildPlatformGuard : BuildPlayerProcessor
{
    public override int callbackOrder => -1000;

    public static void RequireActiveTarget(BuildTarget requested)
    {
        if (EditorUserBuildSettings.activeBuildTarget != requested)
            throw new BuildFailedException(
                $"Switch the active build platform to {requested} and wait for imports before building. " +
                $"The editor is currently targeting {EditorUserBuildSettings.activeBuildTarget}; " +
                "otherwise Addressables can package incompatible localization bundles.");
    }

    public override void PrepareForBuild(BuildPlayerContext context)
    {
        RequireActiveTarget(context.BuildPlayerOptions.target);
    }
}

// AddressablesPlayerBuildProcessor runs at order 1. Inspect its actual output.
public sealed class ReleaseAddressablesPlatformGuard : BuildPlayerProcessor
{
    public override int callbackOrder => 2;

    public override void PrepareForBuild(BuildPlayerContext context)
    {
        var path = Path.Combine(Addressables.BuildPath, "settings.json");
        if (!File.Exists(path))
            throw new BuildFailedException("Addressables settings are missing. Build localization content before the player.");

        var data = JsonUtility.FromJson<ResourceManagerRuntimeData>(File.ReadAllText(path));
        var expected = context.BuildPlayerOptions.target.ToString();
        if (data == null || data.BuildTarget != expected)
            throw new BuildFailedException($"Addressables content does not match {expected}. Rebuild content for this platform.");
    }
}
#endif
