// Assets/Editor/BuildScript.cs
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    public static void BuildiOS()
    {
        // Куда экспортировать Xcode-проект (берём из env, как в codemagic.yaml)
        var exportPath = Environment.GetEnvironmentVariable("IOS_EXPORT_PATH");
        if (string.IsNullOrEmpty(exportPath)) exportPath = "build/ios";
        Directory.CreateDirectory(exportPath);

        // (необязательно) Проставим bundle id из env
        var bundleId = Environment.GetEnvironmentVariable("APP_BUNDLE_ID");
        if (!string.IsNullOrEmpty(bundleId))
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, bundleId);
        }

        // Выберем сцены из Build Settings
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new Exception("No scenes are enabled in Build Settings.");

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            target = BuildTarget.iOS,
            locationPathName = exportPath,
            // Никаких CleanBuild — такого флага нет. Если нужно чистить кеш,
            // делай это отдельным шагом, а тут оставим None.
            options = BuildOptions.None
        };

        Debug.Log($"[CI] Export iOS to: {exportPath}");
        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception($"Build failed: {report.summary.result}");
    }
}
