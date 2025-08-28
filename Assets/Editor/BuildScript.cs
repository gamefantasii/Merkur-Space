using System;
using System.IO;
using System.Linq;
using UnityEditor;

public static class BuildScript
{
    public static void BuildiOS()
    {
        var outDir = Environment.GetEnvironmentVariable("IOS_EXPORT_PATH");
        if (string.IsNullOrEmpty(outDir)) outDir = "build/ios";
        Directory.CreateDirectory(outDir);

        var bundleId = Environment.GetEnvironmentVariable("APP_BUNDLE_ID");
        if (!string.IsNullOrEmpty(bundleId))
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, bundleId);

        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenes.Length == 0) throw new Exception("No enabled scenes in Build Settings.");

        var opts = new BuildPlayerOptions {
            scenes = scenes, locationPathName = outDir, target = BuildTarget.iOS, options = BuildOptions.CleanBuild
        };
        var report = BuildPipeline.BuildPlayer(opts);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new Exception("Unity iOS export failed: " + report.summary.result);
        Debug.Log("iOS project exported to: " + outDir);
    }
}
