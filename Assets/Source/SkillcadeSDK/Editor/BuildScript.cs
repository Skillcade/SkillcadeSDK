using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkillcadeSDK.Connection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SkillcadeSDK.Editor
{
    public static class BuildScript
    {
        private const string BuildConfigArgument = "-buildConfig";
        private const string DefaultBuildPath = "Builds/";
        private const string SkillcadeDebugDefine = "SKILLCADE_DEBUG";

        [MenuItem("Assets/Build Configuration/Build From Selected Config", isValidateFunction: false)]
        public static void BuildFromSelectedConfig()
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject is BuildConfiguration config)
            {
                BuildFromConfig(config);
            }
            else
            {
                Debug.LogError("No BuildConfiguration selected.");
                EditorUtility.DisplayDialog("Build Error", "Please select a BuildConfiguration asset first.", "OK");
            }
        }
        
        [MenuItem("Assets/Build Configuration/Build From Selected Config", isValidateFunction: true)]
        public static bool BuildFromSelectedConfigValidation()
        {
            return Selection.activeObject is BuildConfiguration;
        }

        [MenuItem("Assets/Build Configuration/Open Build Folder", isValidateFunction: false)]
        public static void OpenBuildFolder()
        {
            var config = Selection.activeObject as BuildConfiguration;
            var buildPath = Path.GetFullPath(Path.Combine(DefaultBuildPath, config.BuildFolderName));
            EditorUtility.RevealInFinder(buildPath);
        }

        [MenuItem("Assets/Build Configuration/Open Build Folder", isValidateFunction: true)]
        public static bool OpenBuildFolderValidation()
        {
            return Selection.activeObject is BuildConfiguration;
        }

        public static void BuildFromCommandLine()
        {
            if (TryGetArgumentValue(BuildConfigArgument, out var configPath))
            {
                var config = AssetDatabase.LoadAssetAtPath<BuildConfiguration>(configPath);
                if (config != null)
                {
                    BuildFromConfig(config, true);
                }
                else
                {
                    Debug.LogError($"BuildConfiguration not found at path: {configPath}");
                    EditorApplication.Exit(1);
                }
            }
            else
            {
                Debug.LogError($"Missing argument {BuildConfigArgument}");
                EditorApplication.Exit(1);
            }
        }

        private static void BuildFromConfig(BuildConfiguration config, bool skipLogs = false)
        {
            if (config.ConnectionConfig == null && !string.IsNullOrEmpty(config.ConnectionConfigName))
            {
                config.ConnectionConfig = FindConnectionConfig(config.ConnectionConfigName);
                if (config.ConnectionConfig != null && !skipLogs)
                    Debug.Log($"[BuildScript] Restored ConnectionConfig via AssetDatabase: {config.ConnectionConfig.name}");
            }

            if (config.ConnectionConfig == null && config.PipelineType != BuildPipelineType.ReplayViewer)
            {
                Debug.LogError("[BuildScript] Connection config is null!");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"Building from config: {config.name}, filename: {config.BuildFileName}, folder: {config.BuildFolderName}");

            // 1. Setup Scene (ConnectionConfig & internal SceneNames)
            SetupBuildEnvironment(config);

            // 2. Prepare Build Settings
            var buildPlayerOptions = new BuildPlayerOptions();
            
            // Collect scenes: Pipeline Start Scene + Config Scenes + Extra Build Scenes
            var scenes = new List<string>();

            if (config.PipelineType == BuildPipelineType.MainGame)
            {
                scenes.Add(Utils.BootstrapScenePath);
            }
            else if (config.PipelineType == BuildPipelineType.ReplayViewer)
            {
                scenes.Add(Utils.ReplaysScenePath);
            }

            // Add scenes from GameScope logic (if valid)
            if (config.SceneNames != null)
            {
                foreach (var sceneName in config.SceneNames)
                {
                     var path = FindScenePath(sceneName);
                     if (!string.IsNullOrEmpty(path))
                         scenes.Add(path);
                }
            }
            
            // Add scenes from ConnectionConfig logic
            if (config.ConnectionConfig != null && config.ConnectionConfig.SceneNames != null)
            {
                foreach (var sceneName in config.ConnectionConfig.SceneNames)
                {
                    var path = FindScenePath(sceneName);
                    if (!string.IsNullOrEmpty(path) && !scenes.Contains(path))
                        scenes.Add(path);
                }
            }

            // Add extra build scenes
            if (config.ExtraBuildScenes != null)
            {
                foreach (var sceneName in config.ExtraBuildScenes)
                {
                    var path = FindScenePath(sceneName);
                    if (!string.IsNullOrEmpty(path) && !scenes.Contains(path))
                        scenes.Add(path);
                }
            }

            buildPlayerOptions.scenes = scenes.ToArray();
            buildPlayerOptions.target = config.BuildTarget;
            
            if (config.BuildTarget != BuildTarget.WebGL)
            {
                buildPlayerOptions.subtarget = (int)config.BuildSubtarget;
            }

            var buildPath = Path.Combine(DefaultBuildPath, config.BuildFolderName);
            if (!Directory.Exists(buildPath))
                Directory.CreateDirectory(buildPath);
            
            buildPlayerOptions.locationPathName = Path.Combine(buildPath, config.BuildFileName);
            
            var options = BuildOptions.CleanBuildCache;
            if (config.DevelopmentBuild)
                options |= BuildOptions.Development;
            
            buildPlayerOptions.options = options;

            // 3. Apply Defines
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(config.BuildTarget);
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
            var originalDefines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            var newDefinesList = originalDefines.Split(';').ToList();
            
            if (config.UseSkillcadeDebug)
            {
                if (!newDefinesList.Contains(SkillcadeDebugDefine))
                    newDefinesList.Add(SkillcadeDebugDefine);
            }
            else
            {
                if (newDefinesList.Contains(SkillcadeDebugDefine))
                    newDefinesList.Remove(SkillcadeDebugDefine);
            }

            var newDefines = string.Join(';', newDefinesList);
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, newDefines);
            
            if (!skipLogs)
                Debug.Log($"Result defines: {newDefines}");
            
            // 4. Build
            if (!skipLogs)
                Debug.Log($"Building to: {buildPlayerOptions.locationPathName}");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            
            // Restore defines
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, originalDefines);

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"Build failed: {report.summary.result}");
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
            }
            else
            {
                if (!skipLogs)
                    Debug.Log($"Build {config.name} succeeded");
                
                if (Application.isBatchMode)
                    EditorApplication.Exit(0);
            }
        }

        private static void SetupBuildEnvironment(BuildConfiguration config)
        {
            if (config.PipelineType == BuildPipelineType.ReplayViewer)
            {
                Debug.Log($"[BuildScript] Skip setup environment - wrong pipeline type {config.PipelineType}");
                return;
            }
            
            Utils.SaveCurrentSceneIfDirty();
            if (!Utils.TryLoadBootstrapSceneAndGetScope(out var scene, out var gameScope))
            {
                Debug.LogError("[BuildScript] Can't get bootstrap scene or get scope");
                return;
            }

            var so = new SerializedObject(gameScope);

            Debug.Log("[BuildScript] applying scope config");
            Utils.ApplyConnectionConfigToGameScope(config.ConnectionConfig, so);

            Debug.Log("[BuildScript] Applying modified properties");
            so.ApplyModifiedProperties();

            if (gameScope.Config == null)
            {
                Debug.LogError("[BuildScript] scope config is null");
                EditorApplication.Exit(1);
                return;
            }
            
            Debug.Log($"[BuildScript] scope config: {gameScope.Config.name}");
            
            EditorUtility.SetDirty(gameScope);
            PrefabUtility.RecordPrefabInstancePropertyModifications(gameScope);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static string FindScenePath(string sceneName)
        {
            var guids = AssetDatabase.FindAssets($"{sceneName} t:Scene");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == sceneName)
                    return path;
            }
            return null;
        }

        private static ConnectionConfig FindConnectionConfig(string configName)
        {
            var guids = AssetDatabase.FindAssets($"{configName} t:ConnectionConfig");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == configName)
                    return AssetDatabase.LoadAssetAtPath<ConnectionConfig>(path);
            }
            return null;
        }

        private static bool TryGetArgumentValue(string argumentName, out string value)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], argumentName) && i + 1 < args.Length)
                {
                    value = args[i + 1];
                    return true;
                }
            }
            value = null;
            return false;
        }
    }
}
