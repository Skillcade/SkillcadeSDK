using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace SkillcadeSDK.DI
{
    public class AdditionalSceneInstaller : MonoInstaller
    {
        [SerializeField] private string _sceneName;
        
        private List<GameObject> _loadedSceneInjectGameObjects;

        public override void Prepare()
        {
            base.Prepare();
            SceneManager.LoadScene(_sceneName, LoadSceneMode.Additive);
        }

        public override void Install(IContainerBuilder builder)
        {
            return;
            _loadedSceneInjectGameObjects = new List<GameObject>();
            Debug.Log($"[AdditionalSceneInstaller] Loading scene {_sceneName}");
            SceneManager.LoadScene(_sceneName, LoadSceneMode.Additive);
            Debug.Log($"[AdditionalSceneInstaller] Scene {_sceneName} loaded");

            bool installerFound = false;
            var scene = SceneManager.GetSceneByName(_sceneName);
            SceneManager.SetActiveScene(scene);
            var rootObjects = scene.GetRootGameObjects();
            Debug.Log($"[AdditionalSceneInstaller] Root objects: {rootObjects.Length}");
            foreach (var rootGameObject in rootObjects)
            {
                Debug.Log($"[AdditionalSceneInstaller] Process scene root object {rootGameObject.name}");
                foreach (var installer in rootGameObject.GetComponents<MonoInstaller>())
                {
                    Debug.Log("[AdditionalSceneInstaller] found installer");
                    installerFound = true;
                    installer.Install(builder);
                    _loadedSceneInjectGameObjects.AddRange(installer.GetAutoInjectGameObjects());
                }
            }

            Debug.Log($"[AdditionalSceneInstaller] found installer at {_sceneName}: {installerFound}");
        }

        public override IEnumerable<GameObject> GetAutoInjectGameObjects()
        {
            foreach (var autoInjectGameObject in _autoInjectGameObjects)
            {
                yield return autoInjectGameObject;
            }

            foreach (var autoInjectGameObject in _loadedSceneInjectGameObjects)
            {
                yield return autoInjectGameObject;
            }
        }
    }
}