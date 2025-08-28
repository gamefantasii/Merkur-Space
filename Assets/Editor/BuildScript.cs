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
        // Путь экспорта Xcode-проекта (берём из переменной окружения из YAML)
        var exportPath = Environment.GetEnvironmentVariable("IOS_EXPORT_PATH");
        if (string.IsNullOrEmpty(exportPath)) exportPath = "build/ios";
        Directory.CreateDirectory(exportPath);

        // (опционально) Проставим bundle id из env
        var bundleId = Environment.GetEnvironmentVariable("APP_BUNDLE_ID");
        if (!string.IsNullOrEmpty(bundleId))
        {
            // Современный API (убирает warning)
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, bundleId);
        }

        // Сцены из Build Settings
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new Exception("No scenes are enabled in Build Settings.");

        var opts = new BuildPlayerOptions
        {
            scenes = scenes,
            target = BuildTarget.iOS,
            locationPathName = exportPath,
            options = BuildOptions.None // Никаких CleanBuild — такого флага в Unity 6 нет
        };

        Debug.Log($"[CI] Export iOS to: {exportPath}");
        var report = BuildPipeline.BuildPlayer(opts);

        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception($"Build failed: {report.summary.result}");
    }
}
