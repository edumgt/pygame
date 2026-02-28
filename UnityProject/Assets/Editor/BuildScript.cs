using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildScript
{
    private static BuildTarget GetDefaultBuildTarget()
    {
        return Application.platform == RuntimePlatform.WindowsEditor
            ? BuildTarget.StandaloneWindows64
            : BuildTarget.StandaloneLinux64;
    }

    private static string[] ResolveScenesToBuild()
    {
        string[] enabledScenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (enabledScenes.Length > 0)
        {
            return enabledScenes;
        }

        string[] anyScenes = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (anyScenes.Length > 0)
        {
            return new[] { anyScenes[0] };
        }

        const string scenesDir = "Assets/Scenes";
        const string defaultScenePath = scenesDir + "/Main.unity";
        Directory.CreateDirectory(scenesDir);
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, defaultScenePath);
        AssetDatabase.SaveAssets();

        return new[] { defaultScenePath };
    }

    public static void PerformBuild()
    {
        string[] scenesToBuild = ResolveScenesToBuild();

        BuildTarget target = GetDefaultBuildTarget();
        string outputPath = target == BuildTarget.StandaloneWindows64
            ? "Builds/Windows/CarRacing.exe"
            : "Builds/Linux/CarRacing.x86_64";

        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-customBuildPath" && i + 1 < args.Length)
            {
                outputPath = args[i + 1];
            }
            else if (args[i] == "-customBuildTarget" && i + 1 < args.Length)
            {
                if (!Enum.TryParse(args[i + 1], true, out target))
                {
                    throw new Exception("Invalid -customBuildTarget: " + args[i + 1]);
                }
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");

        var options = new BuildPlayerOptions
        {
            scenes = scenesToBuild,
            locationPathName = outputPath,
            target = target,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception("Build failed: " + report.summary.result);
        }

        Console.WriteLine("Build succeeded: " + outputPath);
    }
}
