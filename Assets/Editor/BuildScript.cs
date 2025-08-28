// Assets/Editor/Build.cs
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildScript
{
    [MenuItem("CI/Build iOS")]
    public static void BuildiOS()
    {
        // Куда выгружать Xcode-проект
        var exportPath = Environment.GetEnvironmentVariable("IOS_EXPORT_PATH");
        if (string.IsNullOrEmpty(exportPath)) exportPath = "build/ios";

        // Попробуем запустить EDM4U Force Resolve, чтобы сгенерировался Podfile
        TryForceResolve();

        // Список сцен
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenes.Length == 0) throw new Exception("No scenes in Build Settings.");

        // Чистим/готовим папку
        if (Directory.Exists(exportPath)) Directory.Delete(exportPath, true);
        Directory.CreateDirectory(exportPath);

        // Сам билд (для iOS всегда экспорт Xcode-проекта)
        var opts = new BuildPlayerOptions {
            scenes = scenes,
            locationPathName = exportPath,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };
        var report = BuildPipeline.BuildPlayer(opts);
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception($"iOS build failed: {report.summary.result}");
        Console.WriteLine($"[BuildScript] Exported Xcode project to: {exportPath}");
    }

    // Пытаемся вызвать EDM4U iOS/PlayServices резолвер (если подключён в проекте)
    static void TryForceResolve()
    {
        try
        {
            // Google.IOSResolver.Resolver.ForceResolve()
            var iosResolver = Type.GetType("Google.IOSResolver, Google.IOSResolver", throwOnError: false);
            var resolverProp = iosResolver?.GetProperty("Resolver", BindingFlags.Public | BindingFlags.Static);
            var resolver = resolverProp?.GetValue(null);
            var force = resolver?.GetType().GetMethod("ForceResolve", BindingFlags.Public | BindingFlags.Instance);
            force?.Invoke(resolver, null);
        }
        catch {}

        try
        {
            // GooglePlayServices.PlayServicesResolver.DoResolution()
            var t = Type.GetType("GooglePlayServices.PlayServicesResolver, Google.JarResolver", throwOnError: false)
                 ?? Type.GetType("GooglePlayServices.PlayServicesResolver, ExternalDependencyManager", throwOnError: false);
            var m = t?.GetMethod("DoResolution", BindingFlags.Public | BindingFlags.Static);
            m?.Invoke(null, null);
        }
        catch {}
    }
}
