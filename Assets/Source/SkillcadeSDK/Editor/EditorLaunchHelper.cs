using System.IO;
using SkillcadeSDK.Connection;
using SkillcadeSDK.DI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SkillcadeSDK.Editor
{
    /// <summary>
    /// Helper class for setting up connection configs when launching from Unity Editor
    /// </summary>
    public static class EditorLaunchHelper
    {
        private const string BootstrapScenePath = "Assets/Scenes/BootstrapScene.unity";

        #region Editor Launch Configuration Menu Items

        [MenuItem("Editor Launch/Setup LocalHost Config")]
        public static void SetupLocalHostConfig()
        {
            SetConnectionConfig("LocalHost");
        }

        [MenuItem("Editor Launch/Setup London Config")]
        public static void SetupLondonConfig()
        {
            SetConnectionConfig("London");
        }

        [MenuItem("Editor Launch/Setup SkillcadeHub Config")]
        public static void SetupSkillcadeHubConfig()
        {
            SetConnectionConfig("SkillcadeHub");
        }

        [MenuItem("Editor Launch/Setup SinglePlayer Config")]
        public static void SetupSinglePlayerConfig()
        {
            SetConnectionConfig("SinglePlayer");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Loads a connection config from Resources and sets it in GameScopeWithAdditionalScenes
        /// </summary>
        /// <param name="configName">Name of the config file (without extension)</param>
        public static void SetConnectionConfig(string configName)
        {
            // Load config from Resources
            var config = Resources.Load<ConnectionConfig>($"Configs/Connection/{configName}");
            if (config == null)
            {
                Debug.LogError($"[EditorLaunchHelper] Connection config '{configName}' not found in Resources/Configs/Connection/");
                EditorUtility.DisplayDialog(
                    "Config Not Found",
                    $"Connection config '{configName}' not found in Resources/Configs/Connection/\n\n" +
                    "Please ensure the config exists in the Resources folder.",
                    "OK");
                return;
            }

            Debug.Log($"[EditorLaunchHelper] Loaded connection config: {configName}");

            // Find BootstrapScene
            if (!File.Exists(BootstrapScenePath))
            {
                Debug.LogError($"[EditorLaunchHelper] BootstrapScene not found at: {BootstrapScenePath}");
                EditorUtility.DisplayDialog(
                    "BootstrapScene Not Found",
                    $"BootstrapScene not found at: {BootstrapScenePath}\n\n" +
                    "Please ensure the scene exists in the project.",
                    "OK");
                return;
            }

            // Check if we need to save current scene
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                bool saveCurrentScene = EditorUtility.DisplayDialog(
                    "Save Current Scene?",
                    "The current scene has unsaved changes. Do you want to save before switching configs?",
                    "Save",
                    "Don't Save");

                if (saveCurrentScene)
                {
                    EditorSceneManager.SaveOpenScenes();
                }
            }

            // Open BootstrapScene
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
                Debug.LogError("[EditorLaunchHelper] GameScopeWithAdditionalScenes not found in BootstrapScene");
                EditorUtility.DisplayDialog(
                    "GameScope Not Found",
                    "GameScopeWithAdditionalScenes component not found in BootstrapScene.\n\n" +
                    "Please ensure the component exists in the scene.",
                    "OK");
                return;
            }

            // Set the connection config using SerializedObject for proper serialization
            var so = new SerializedObject(gameScope);
            var connectionConfigProperty = so.FindProperty("_connectionConfig");

            if (connectionConfigProperty == null)
            {
                Debug.LogError("[EditorLaunchHelper] _connectionConfig property not found in GameScopeWithAdditionalScenes");
                EditorUtility.DisplayDialog(
                    "Property Not Found",
                    "_connectionConfig property not found in GameScopeWithAdditionalScenes.\n\n" +
                    "Please ensure the SDK is up to date.",
                    "OK");
                return;
            }

            connectionConfigProperty.objectReferenceValue = config;
            so.ApplyModifiedProperties();

            // Mark object as dirty first, then scene, then save
            EditorUtility.SetDirty(gameScope);
            EditorUtility.SetDirty(gameScope.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);

            // Save the scene
            if (!EditorSceneManager.SaveScene(scene))
            {
                Debug.LogError("[EditorLaunchHelper] Failed to save BootstrapScene");
                EditorUtility.DisplayDialog(
                    "Save Failed",
                    "Failed to save BootstrapScene. Please try again.",
                    "OK");
                return;
            }

            // Force asset database refresh to ensure changes are persisted
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Verify the config was set correctly by reading it back
            so.Update();
            var verifyConfig = connectionConfigProperty.objectReferenceValue as ConnectionConfig;
            if (verifyConfig == null || verifyConfig != config)
            {
                Debug.LogError("[EditorLaunchHelper] Config verification failed - config did not persist!");
                EditorUtility.DisplayDialog(
                    "Verification Failed",
                    "The config was set but did not persist correctly.\n\n" +
                    "This may be a Unity serialization issue. Please try again or set the config manually.",
                    "OK");
                return;
            }

            // Select the GameScope object in hierarchy for visibility
            Selection.activeGameObject = gameScope.gameObject;

            Debug.Log($"[EditorLaunchHelper] Successfully set connection config to: {configName}");
            Debug.Log($"[EditorLaunchHelper] Config details - Server: {config.ServerAddress}:{config.ServerListenPort}");
            Debug.Log($"[EditorLaunchHelper] Config verified and persisted successfully");

            // Show success dialog
            string sceneInfo = config.SceneNames != null && config.SceneNames.Length > 0
                ? $"\n\nScenes to load: {string.Join(", ", config.SceneNames)}"
                : "";

            EditorUtility.DisplayDialog(
                "Config Set Successfully",
                $"Connection config set to: {configName}\n" +
                $"Server: {config.ServerAddress}:{config.ServerListenPort}" +
                sceneInfo +
                "\n\nBootstrapScene has been saved.\n" +
                "You can now press Play to launch with this configuration.",
                "OK");
        }

        /// <summary>
        /// Gets the currently set connection config from the BootstrapScene
        /// </summary>
        /// <returns>The current ConnectionConfig, or null if not set</returns>
        public static ConnectionConfig GetCurrentConfig()
        {
            if (!File.Exists(BootstrapScenePath))
            {
                Debug.LogWarning($"[EditorLaunchHelper] BootstrapScene not found at: {BootstrapScenePath}");
                return null;
            }

            var scene = EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            var rootObjects = scene.GetRootGameObjects();
            GameScopeWithAdditionalScenes gameScope = null;

            foreach (var rootObject in rootObjects)
            {
                gameScope = rootObject.GetComponent<GameScopeWithAdditionalScenes>();
                if (gameScope != null) break;
            }

            if (gameScope == null)
            {
                Debug.LogWarning("[EditorLaunchHelper] GameScopeWithAdditionalScenes not found in BootstrapScene");
                return null;
            }

            var so = new SerializedObject(gameScope);
            var connectionConfigProperty = so.FindProperty("_connectionConfig");

            return connectionConfigProperty?.objectReferenceValue as ConnectionConfig;
        }

        [MenuItem("Editor Launch/Show Current Config")]
        public static void ShowCurrentConfig()
        {
            var currentConfig = GetCurrentConfig();

            if (currentConfig == null)
            {
                EditorUtility.DisplayDialog(
                    "No Config Set",
                    "No connection config is currently set in BootstrapScene.\n\n" +
                    "Use 'Editor Launch' menu to set a config before playing.",
                    "OK");
                return;
            }

            string sceneInfo = currentConfig.SceneNames != null && currentConfig.SceneNames.Length > 0
                ? $"\nScenes: {string.Join(", ", currentConfig.SceneNames)}"
                : "\nNo additional scenes configured";

            EditorUtility.DisplayDialog(
                "Current Config",
                $"Config: {currentConfig.name}\n" +
                $"Server: {currentConfig.ServerAddress}:{currentConfig.ServerListenPort}\n" +
                $"Encryption: {currentConfig.UseEncryption}\n" +
                $"SkillcadeHub: {currentConfig.SkillcadeHubIntegrated}" +
                sceneInfo,
                "OK");
        }

        [MenuItem("Editor Launch/Debug - Verify Config Persistence")]
        public static void DebugVerifyConfigPersistence()
        {
            if (!File.Exists(BootstrapScenePath))
            {
                Debug.LogError($"[EditorLaunchHelper] BootstrapScene not found at: {BootstrapScenePath}");
                return;
            }

            // Save current scene if needed
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                EditorSceneManager.SaveOpenScenes();
            }

            // Close and reopen scene to simulate editor reload
            var currentScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);

            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            GameScopeWithAdditionalScenes gameScope = null;

            foreach (var rootObject in rootObjects)
            {
                gameScope = rootObject.GetComponent<GameScopeWithAdditionalScenes>();
                if (gameScope != null) break;
            }

            if (gameScope == null)
            {
                Debug.LogError("[EditorLaunchHelper] GameScopeWithAdditionalScenes not found in BootstrapScene");
                return;
            }

            var so = new SerializedObject(gameScope);
            var connectionConfigProperty = so.FindProperty("_connectionConfig");
            var config = connectionConfigProperty?.objectReferenceValue as ConnectionConfig;

            if (config == null)
            {
                Debug.LogError("[EditorLaunchHelper] VERIFICATION FAILED: Config is NULL after scene reload!");
                EditorUtility.DisplayDialog(
                    "Verification Failed",
                    "Config is NULL after scene reload.\n\n" +
                    "This indicates a serialization issue. The config reference is not being saved properly.",
                    "OK");
            }
            else
            {
                Debug.Log($"[EditorLaunchHelper] VERIFICATION PASSED: Config '{config.name}' persisted correctly!");
                Debug.Log($"[EditorLaunchHelper] Server: {config.ServerAddress}:{config.ServerListenPort}");

                // Also check using direct field access
                var so2 = new SerializedObject(gameScope);
                Debug.Log($"[EditorLaunchHelper] SerializedObject verification: {so2.FindProperty("_connectionConfig").objectReferenceValue}");

                EditorUtility.DisplayDialog(
                    "Verification Passed",
                    $"Config '{config.name}' persisted correctly!\n\n" +
                    $"Server: {config.ServerAddress}:{config.ServerListenPort}",
                    "OK");
            }
        }

        #endregion
    }
}
