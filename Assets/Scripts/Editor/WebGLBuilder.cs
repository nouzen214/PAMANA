using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

namespace TopDownGame.Editor
{
    public static class WebGLBuilder
    {
        [MenuItem("Tools/Build/Build WebGL Player")]
        public static void BuildWebGL()
        {
            var scenes = EditorBuildSettings.scenes;
            string[] scenePaths = new string[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
            {
                scenePaths[i] = scenes[i].path;
            }

            string buildPath = "Builds/WebGL";
            PlayerSettings.WebGL.decompressionFallback = true;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = scenePaths;
            buildPlayerOptions.locationPathName = buildPath;
            buildPlayerOptions.target = BuildTarget.WebGL;
            buildPlayerOptions.options = BuildOptions.None;

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[WebGLBuilder] Build succeeded: {summary.totalSize} bytes");
                CopyDirectory("Builds/WebGL/Build", "docs/Build");
                CopyDirectory("Builds/WebGL/TemplateData", "docs/TemplateData");
                File.Copy("Builds/WebGL/index.html", "docs/index.html", true);
                if (!File.Exists("docs/.nojekyll"))
                {
                    File.WriteAllText("docs/.nojekyll", "");
                }
                Debug.Log("[WebGLBuilder] Synced build to docs/ folder!");
            }
            else
            {
                Debug.LogError($"[WebGLBuilder] Build failed with result: {summary.result}");
            }
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string targetFilePath = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, targetFilePath, true);
            }
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string nextDestinationDir = Path.Combine(destinationDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, nextDestinationDir);
            }
        }
    }
}
