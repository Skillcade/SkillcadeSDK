using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace SkillcadeSDK.DI
{
    public class GameScopeWithAdditionalScenes : LifetimeScope
    {
        [SerializeField] private string[] _sceneNames;
        [SerializeField] private MonoInstaller[] _rootInstallers;
        
        private List<MonoInstaller> _loadedInstallers;
        
        protected override void Awake()
        {
            LoadScenesAndBuildAsync();
        }

        private async void LoadScenesAndBuildAsync()
        {
            foreach (var sceneName in _sceneNames)
            {
                await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }
            
            _loadedInstallers = new List<MonoInstaller>();
            foreach (var sceneName in _sceneNames)
            {
                var scene = SceneManager.GetSceneByName(sceneName);
                if (!scene.IsValid())
                {
                    Debug.LogError($"[GameScopeWithAdditionalScenes] Scene {sceneName} not valid");
                    continue;
                }

                if (!scene.isLoaded)
                {
                    Debug.LogError($"[GameScopeWithAdditionalScenes] Scene {sceneName} not loaded");
                    continue;
                }
                
                var rootObjects = scene.GetRootGameObjects();
                Debug.Log($"[GameScopeWithAdditionalScenes] Scene {sceneName} has {rootObjects.Length} objects");
                foreach (var rootObject in rootObjects)
                {
                    _loadedInstallers.AddRange(rootObject.GetComponents<MonoInstaller>());
                }
            }
            
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            
            builder.Register<ContainerSingletonWrapper>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.RegisterBuildCallback(AutoInjectTargets);
            
            foreach (var installer in _rootInstallers)
            {
                Debug.Log($"[GameScopeWithAdditionalScenes] install {installer.GetType().Name}");
                installer.Install(builder);
            }
            
            foreach (var installer in _loadedInstallers)
            {
                Debug.Log($"[GameScopeWithAdditionalScenes] install {installer.GetType().Name}");
                installer.Install(builder);
            }
        }

        private void AutoInjectTargets(IObjectResolver objectResolver)
        {
            foreach (var installer in _rootInstallers)
            {
                foreach (var autoInjectGameObject in installer.GetAutoInjectGameObjects())
                {
                    objectResolver.InjectGameObject(autoInjectGameObject);
                }
            }
            
            foreach (var installer in _loadedInstallers)
            {
                foreach (var autoInjectGameObject in installer.GetAutoInjectGameObjects())
                {
                    objectResolver.InjectGameObject(autoInjectGameObject);
                }
            }
        }
    }
}