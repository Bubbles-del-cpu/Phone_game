# Release platform checks

Before each player build, switch the Unity Editor active build target to the desired platform and wait for asset imports and compilation to finish. Do not rely on BuildPipeline.BuildPlayer to switch targets: Addressables prepares its bundles using the previously active editor target before that switch.

Addressables is configured to Build With Player. ReleaseBuildPlatformGuard fails builds that request another platform before the switch. ReleaseAddressablesPlatformGuard checks the generated settings after the Addressables processor runs.

Build each platform into a separate clean output folder. Package desktop players into separate ZIP files preserving executable permissions; keep Android as an APK.

Before distributing, run the archive validator for EACH final file:

```sh
python3 Tools/Codex/verify_release_addressables.py "NTS Honeymoon 0.21.1 Windows.zip" Windows
python3 Tools/Codex/verify_release_addressables.py "NTS Honeymoon 0.21.1 Linux.zip" Linux
python3 Tools/Codex/verify_release_addressables.py "NTS Honeymoon 0.21.1 macOS.zip" macOS
python3 Tools/Codex/verify_release_addressables.py "NTS Honeymoon 0.21.1 Android.apk" Android
```

The validator checks the packaged runtime build target, platform names in the catalog, bundle directories, and the platform IDs inside every serialized bundle file. It uses Unity's WebExtract tool and supports the serialized format used by this project's Unity 6000.2.8f1. Set UNITY_WEBEXTRACT to its path on other installations. A passing archive-integrity check alone does not detect cross-platform bundles.

Finish with built-player smoke tests for startup, locale initialization, and chapter loading on each target OS when available.
