// Assets/Editor/BuildScript.cs
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    // Ровно это имя дергает Codemagic: BuildScript.BuildiOS
    public static void BuildiOS()
    {
        // Куда экспортировать (берём из ENV, fallback — build/ios)
        var exportPath = Environment.GetEnvironmentVariable("IOS_EXPORT_PATH") ?? "build/ios";
        Directory.CreateDirectory(exportPath);

        // ID бандла (если задан через ENV)
        var bundleId = Environment.GetEnvironmentVariable("BUNDLE_ID");
        if (!string.IsNullOrEmpty(bundleId))
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, bundleId);
        }

        // Сцены из Build Settings
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();
        if (scenes.Length == 0)
            throw new Exception("No scenes enabled in Build Settings.");

        // Опции билда (без устаревшего CleanBuild)
        var options = BuildOptions.None;

        var buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = exportPath, // для iOS — путь к папке проекта Xcode
            target = BuildTarget.iOS,
            options = options
        };

        var report = BuildPipeline.BuildPlayer(buildOptions);
        var summary = report.summary;

        Debug.Log($"[BuildiOS] Result={summary.result}, Output={exportPath}, Size={summary.totalSize} bytes");

        if (summary.result != BuildResult.Succeeded)
            throw new Exception($"Unity iOS build failed: {summary.result}");
    }
}
