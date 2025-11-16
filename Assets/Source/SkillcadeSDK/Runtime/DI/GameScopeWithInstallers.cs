using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SkillcadeSDK.DI
{
    public class GameScopeWithInstallers : LifetimeScope
    {
        [SerializeField] private MonoInstaller[] _installers;
        
        protected override void Configure(IContainerBuilder builder)
        {
            EntryPointsBuilder.EnsureDispatcherRegistered(builder);
            
            foreach (var installer in _installers)
            {
                if (installer != null)
                    installer.Install(builder);
            }

            base.Configure(builder);
        }
    }
}