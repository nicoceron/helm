#if UNITY_EDITOR
using System.IO;
using Lionrise;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LionriseEditor
{
    [InitializeOnLoad]
    public static class ProjectBootstrap
    {
        private const string BootPath = "Assets/_Game/Scenes/Boot.unity";
        private const string GamePath = "Assets/_Game/Scenes/Game.unity";

        static ProjectBootstrap()
        {
            EditorApplication.delayCall += SetupIfNeeded;
        }

        [MenuItem("Tools/Lionrise/Setup Project")]
        public static void SetupIfNeeded()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            Directory.CreateDirectory("Assets/_Game/Scenes");
            CreateEmptyScene(BootPath);
            CreateEmptyScene(GamePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootPath, true),
                new EditorBuildSettingsScene(GamePath, true)
            };
            PlayerSettings.productName = "Lionrise Protocol";
            PlayerSettings.companyName = "Civic Signal";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.defaultScreenWidth = 1024;
            PlayerSettings.defaultScreenHeight = 768;
            PlayerSettings.defaultIsNativeResolution = false;
            PlayerSettings.resizableWindow = false;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.bundleVersion = "0.2.0";
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Tools/Lionrise/Validate Content")]
        public static void ValidateContent()
        {
            var path = Path.Combine(Application.streamingAssetsPath, "Cards", "cards.json");
            if (!File.Exists(path))
            {
                Debug.LogError($"Card file missing: {path}");
                return;
            }
            var collection = JsonUtility.FromJson<CardCollection>(File.ReadAllText(path));
            var report = ContentValidator.Validate(collection);
            foreach (var warning in report.warnings) Debug.LogWarning(warning);
            foreach (var error in report.errors) Debug.LogError(error);
            if (report.IsValid) Debug.Log($"Lionrise content valid: {collection.cards.Length} cards, {report.warnings.Count} warnings.");
        }

        [MenuItem("Tools/Lionrise/Build macOS Player")]
        public static void BuildMacPlayer()
        {
            SetupIfNeeded();
            var output = Path.GetFullPath("Builds/macOS/Lionrise Protocol.app");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { BootPath, GamePath },
                locationPathName = output,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new System.InvalidOperationException($"macOS build failed with {report.summary.totalErrors} error(s).");
            Debug.Log($"Lionrise macOS player built: {output} ({report.summary.totalSize} bytes)");
        }

        private static void CreateEmptyScene(string path)
        {
            if (File.Exists(path)) return;
            var active = SceneManager.GetActiveScene();
            var replaceUntitled = SceneManager.sceneCount == 1 && string.IsNullOrEmpty(active.path);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
                replaceUntitled ? NewSceneMode.Single : NewSceneMode.Additive);
            EditorSceneManager.SaveScene(scene, path);
            if (!replaceUntitled) EditorSceneManager.CloseScene(scene, true);
        }
    }
}
#endif
