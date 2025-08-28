// Assets/Editor/Build.cs
#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildScript
{
    // Unity будет вызывать этот метод: -executeMethod BuildScript.BuildiOS
    public static void BuildiOS()
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenes.Length == 0) throw new System.Exception("No enabled scenes in Build Settings.");

        var outPath = System.Environment.GetEnvironmentVariable("IOS_EXPORT_PATH") ?? "build/ios";
        Directory.CreateDirectory(outPath);

        // Базовые настройки iOS
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 1); // ARM64
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
        PlayerSettings.iOS.targetOSVersionString = "12.0";

        // (опц.) Подменить bundle id из переменной окружения
        var envBundleId = System.Environment.GetEnvironmentVariable("BUNDLE_ID");
        if (!string.IsNullOrEmpty(envBundleId))
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, envBundleId);

        var opts = new BuildPlayerOptions {
            scenes = scenes,
            locationPathName = outPath,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(opts);
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception($"Unity iOS export failed: {report.summary.result}");
    }
}
#endif
