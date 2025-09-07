using System.Linq;
using UnityEditor;
using UnityEngine;

public static class BuildScript {
  [MenuItem("Build/Build iOS")]
  public static void BuildIos() {
    var opts = new BuildPlayerOptions {
      locationPathName = "ios",           // папка для Xcode-проекта
      target = BuildTarget.iOS,
      options = BuildOptions.None,
      scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray()
    };
    Debug.Log("Building iOS");
    BuildPipeline.BuildPlayer(opts);
    Debug.Log("Built iOS");
  }
}
