// Assets/Editor/BuildScript.cs
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.Build; // NamedBuildTarget
#endif
using UnityEngine;

public static class BuildScript
{
    public static void BuildiOS()
    {
        var exportPath = Environment.GetEnvironmentVariable("IOS_EXPORT_PATH") ?? "build/ios";
        Directory.CreateDirectory(exportPath);

        var bundleId = Environment.GetEnvironmentVariable("APP_BUNDLE_ID");
        if (!string.IsNullOrEmpty(bundleId))
        {
#if UNITY_2021_2_OR_NEWER
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, bundleId);
#else
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, bundleId);
#endif
        }

        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenes.Length == 0) throw new Exception("No scenes are enabled in Build Settings.");

        var opts = new BuildPlayerOptions
        {
            target = BuildTarget.iOS,
            locationPathName = exportPath,
            scenes = scenes,
            options = BuildOptions.None
        };

        Debug.Log($"[CI] Export iOS to: {exportPath}");
        var report = BuildPipeline.BuildPlayer(opts);
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception($"Build failed: {report.summary.result}");
    }
}
