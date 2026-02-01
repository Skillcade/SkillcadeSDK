using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkillcadeSDK.Connection;
using SkillcadeSDK.DI;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SkillcadeSDK.Editor
{
    public static class BuildScript
    {
        private const string BuildPathArgument = "-buildPath";
        private const string BuildNameArgument = "-buildName";
        private const string BuildDevelopmentArgument = "-development";
        private const string BootstrapScenePath = "Assets/Scenes/BootstrapScene.unity";

        #region Server Build Methods

        [MenuItem("Build/Server/SkillcadeHub (Edgegap)")]
        public static void BuildSkillcadeHubServer()
        {
            Debug.Log("Building SkillcadeHub Server for Edgegap...");

            // Load and set connection config
            LoadAndSetConnectionConfig("SkillcadeHub");

            // Setup build scenes from config and GameScope
            SetupBuildScenes();

            // Add Edgegap-specific compiler defines
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(BuildTarget.StandaloneLinux64);
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            if (!defines.Contains("EDGEGAP_PLUGIN_SERVERS"))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup,
                    defines + ";EDGEGAP_PLUGIN_SERVERS");
            }

            if (!TryGetArgumentValue(BuildNameArgument, out var buildName))
            {
                buildName = "ServerBuild"; // Default for Edgegap
            }

            // Build with StandaloneBuildSubtarget.Server (critical for Edgegap)
            Build(BuildTarget.StandaloneLinux64, (int)StandaloneBuildSubtarget.Server, buildName);
        }

        [MenuItem("Build/Server/London")]
        public static void BuildLondonServer()
        {
            Debug.Log("Building London Server...");

            LoadAndSetConnectionConfig("London");
            SetupBuildScenes();

            if (!TryGetArgumentValue(BuildNameArgument, out var buildName))
            {
                Debug.LogError("Build name not specified");
                EditorApplication.Exit(1);
                return;
            }

            Build(BuildTarget.StandaloneLinux64, (int)StandaloneBuildSubtarget.Server, buildName);
        }

        #endregion

        #region WebGL Client Build Methods

        [MenuItem("Build/Client/LocalHost")]
        public static void BuildLocalHostClient()
        {
            Debug.Log("Building LocalHost WebGL Client...");

            LoadAndSetConnectionConfig("LocalHost");
            SetupBuildScenes();

            Build(BuildTarget.WebGL);
        }

        [MenuItem("Build/Client/London")]
        public static void BuildLondonClient()
        {
            Debug.Log("Building London WebGL Client...");

            LoadAndSetConnectionConfig("London");
            SetupBuildScenes();

            Build(BuildTarget.WebGL);
        }

        [MenuItem("Build/Client/SkillcadeHub")]
        public static void BuildSkillcadeHubClient()
        {
            Debug.Log("Building SkillcadeHub WebGL Client...");

            LoadAndSetConnectionConfig("SkillcadeHub");
            SetupBuildScenes();

            Build(BuildTarget.WebGL);
        }

        [MenuItem("Build/Client/SinglePlayer")]
        public static void BuildSinglePlayerClient()
        {
            Debug.Log("Building SinglePlayer WebGL Client...");

            LoadAndSetConnectionConfig("SinglePlayer");
            SetupBuildScenes();

            Build(BuildTarget.WebGL);
        }

        #endregion

        #region Helper Methods

        private static void LoadAndSetConnectionConfig(string configName)
        {
            // Load config from Resources
            var config = Resources.Load<ConnectionConfig>($"Configs/Connection/{configName}");
            if (config == null)
            {
                Debug.LogError($"Connection config '{configName}' not found in Resources/Configs/Connection/");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"Loaded connection config: {configName}");

            // Find BootstrapScene
            if (!File.Exists(BootstrapScenePath))
            {
                Debug.LogError($"BootstrapScene not found at: {BootstrapScenePath}");
                EditorApplication.Exit(1);
                return;
            }

            var scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);

            // Find GameScopeWithAdditionalScenes component
            var rootObjects = scene.GetRootGameObjects();
            GameScopeWithAdditionalScenes gameScope = null;

            foreach (var rootObject in rootObjects)
            {
                gameScope = rootObject.GetComponent<GameScopeWithAdditionalScenes>();
                if (gameScope != null) break;
            }

            if (gameScope == null)
            {
                Debug.LogError("GameScopeWithAdditionalScenes not found in BootstrapScene");
                EditorApplication.Exit(1);
                return;
            }

            // Set the connection config using SerializedObject for proper serialization
            var so = new SerializedObject(gameScope);
            so.FindProperty("_connectionConfig").objectReferenceValue = config;
            so.ApplyModifiedProperties();

            // Save scene
            EditorSceneManager.SaveScene(scene);

            Debug.Log($"Connection config set to: {configName}");
        }

        private static void SetupBuildScenes()
        {
            // Open BootstrapScene to get GameScope configuration
            if (!File.Exists(BootstrapScenePath))
            {
                Debug.LogError($"BootstrapScene not found at: {BootstrapScenePath}");
                EditorApplication.Exit(1);
                return;
            }

            var scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);

            // Find GameScopeWithAdditionalScenes component
            var rootObjects = scene.GetRootGameObjects();
            GameScopeWithAdditionalScenes gameScope = null;

            foreach (var rootObject in rootObjects)
            {
                gameScope = rootObject.GetComponent<GameScopeWithAdditionalScenes>();
                if (gameScope != null) break;
            }

            if (gameScope == null)
            {
                Debug.LogError("GameScopeWithAdditionalScenes not found in BootstrapScene");
                EditorApplication.Exit(1);
                return;
            }

            // Collect all scenes from GameScope and ConnectionConfig
            var allScenes = new HashSet<string>();

            // Get scenes from GameScope's _sceneNames
            var so = new SerializedObject(gameScope);
            var sceneNamesProperty = so.FindProperty("_sceneNames");
            if (sceneNamesProperty != null && sceneNamesProperty.isArray)
            {
                for (int i = 0; i < sceneNamesProperty.arraySize; i++)
                {
                    var sceneName = sceneNamesProperty.GetArrayElementAtIndex(i).stringValue;
                    if (!string.IsNullOrEmpty(sceneName))
                    {
                        allScenes.Add(sceneName);
                    }
                }
            }

            // Get scenes from ConnectionConfig's SceneNames
            var connectionConfigProperty = so.FindProperty("_connectionConfig");
            if (connectionConfigProperty != null && connectionConfigProperty.objectReferenceValue != null)
            {
                var config = connectionConfigProperty.objectReferenceValue as ConnectionConfig;
                if (config != null && config.SceneNames != null)
                {
                    foreach (var sceneName in config.SceneNames)
                    {
                        if (!string.IsNullOrEmpty(sceneName))
                        {
                            allScenes.Add(sceneName);
                        }
                    }
                }
            }

            // Build the scene list for EditorBuildSettings
            var sceneList = new List<EditorBuildSettingsScene>();

            // Always include BootstrapScene first
            sceneList.Add(new EditorBuildSettingsScene(BootstrapScenePath, true));

            // Add all collected scenes (they will be loaded additively at runtime)
            foreach (var sceneName in allScenes)
            {
                // Find scene path by name
                var scenePath = FindScenePath(sceneName);
                if (!string.IsNullOrEmpty(scenePath))
                {
                    sceneList.Add(new EditorBuildSettingsScene(scenePath, true));
                }
                else
                {
                    Debug.LogWarning($"Scene '{sceneName}' not found in project. Skipping.");
                }
            }

            EditorBuildSettings.scenes = sceneList.ToArray();
            Debug.Log($"Build scenes configured ({sceneList.Count} scenes): {string.Join(", ", sceneList.Select(s => Path.GetFileNameWithoutExtension(s.path)))}");
        }

        private static string FindScenePath(string sceneName)
        {
            // Search for scene in Assets folder
            var guids = AssetDatabase.FindAssets($"{sceneName} t:Scene");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (fileName == sceneName)
                {
                    return path;
                }
            }

            return null;
        }

        private static void Build(BuildTarget target, int subtarget = 0, string buildName = "")
        {
            if (!TryGetArgumentValue(BuildPathArgument, out string buildPath))
            {
                Debug.LogError("Build path not specified");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"Build path: {buildPath}");
            if (Directory.Exists(buildPath))
            {
                Debug.Log("Deleting old build...");
                Directory.Delete(buildPath, true);
            }

            var buildOptions = BuildOptions.CleanBuildCache;
            if (HasArgument(BuildDevelopmentArgument))
                buildOptions |= BuildOptions.Development;

            string resultPath = Path.Combine(buildPath, buildName);
            var options = new BuildPlayerOptions
            {
                scenes = GetBuildScenes(),
                locationPathName = resultPath,
                target = target,
                subtarget = subtarget,
                options = buildOptions,
            };

            Debug.Log($"Executing build at {resultPath}");
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"Build failed: {report.summary.result}");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("Build succeeded");
            EditorApplication.Exit(0);
        }

        private static string[] GetBuildScenes()
        {
            var scenes = new string[SceneManager.sceneCountInBuildSettings];
            for (var i = 0; i < scenes.Length; i++)
            {
                scenes[i] = SceneUtility.GetScenePathByBuildIndex(i);
            }

            return scenes;
        }

        private static bool TryGetArgumentValue(string argumentName, out string value)
        {
            Debug.Log($"Searching for argument: {argumentName}");
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], argumentName) && i + 1 < args.Length)
                {
                    value = args[i + 1];
                    Debug.Log($"Found argument {argumentName} value: {value}");
                    return true;
                }
            }

            Debug.Log($"Argument {argumentName} not found");
            value = null;
            return false;
        }

        private static bool HasArgument(string argumentName)
        {
            Debug.Log($"Searching for argument: {argumentName}");
            string[] args = Environment.GetCommandLineArgs();
            foreach (var argument in args)
            {
                if (string.Equals(argument, argumentName))
                {
                    Debug.Log($"Found argument {argumentName}");
                    return true;
                }
            }

            Debug.Log($"Argument {argumentName} not found");
            return false;
        }

        #endregion
    }
}
