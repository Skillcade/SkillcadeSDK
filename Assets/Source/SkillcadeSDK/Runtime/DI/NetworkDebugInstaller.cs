#if SKILLCADE_DEBUG
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SkillcadeSDK.DI
{
    public class NetworkDebugInstaller : MonoInstaller
    {
        [SerializeField] private string _debugSceneName;

        public override void Install(IContainerBuilder builder)
        {
            if (string.IsNullOrEmpty(_debugSceneName))
            {
                Debug.LogWarning("[NetworkDebugInstaller] Debug scene name is empty, skipping registration.");
                return;
            }

            builder.RegisterEntryPoint<NetworkDebugScopeLoader>(Lifetime.Singleton)
                .WithParameter(_debugSceneName);
        }
    }
}
#endif
