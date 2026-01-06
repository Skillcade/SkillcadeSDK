using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SkillcadeSDK.DI
{
    public class GameScopeWithInstallers : LifetimeScope
    {
        [SerializeField] private MonoInstaller[] _installers;

        protected override void Awake()
        {
            foreach (var installer in _installers)
            {
                installer.Prepare();
            }
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            EntryPointsBuilder.EnsureDispatcherRegistered(builder);
            
            builder.Register<ContainerSingletonWrapper>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.RegisterBuildCallback(AutoInjectTargets);

            var allInstallers = FindObjectsByType<MonoInstaller>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Debug.Log($"[GameScopeWithInstallers] Found {allInstallers.Length} installers");

            Debug.Log("[GameScopeWithInstallers] Process installers");
            foreach (var installer in allInstallers)
            {
                if (installer != null)
                    installer.Install(builder);
            }

            Debug.Log("[GameScopeWithInstallers] Finish process installers");
            base.Configure(builder);
        }

        private void AutoInjectTargets(IObjectResolver objectResolver)
        {
            foreach (var installer in _installers)
            {
                foreach (var autoInjectGameObject in installer.GetAutoInjectGameObjects())
                {
                    objectResolver.InjectGameObject(autoInjectGameObject);
                }
            }
        }
    }
}