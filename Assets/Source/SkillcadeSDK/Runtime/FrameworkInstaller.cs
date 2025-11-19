using SkillcadeSDK.Common;
using SkillcadeSDK.Common.Level;
using SkillcadeSDK.DI;
using SkillcadeSDK.WebRequests;
using UnityEngine;
using VContainer;

namespace SkillcadeSDK
{
    public class FrameworkInstaller : MonoInstaller
    {
        [SerializeField] private WebBridge _webBridge;
        
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<ContainerSingletonWrapper>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<LayerProvider>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<WebRequester>(Lifetime.Singleton);
            builder.Register<RespawnServiceProvider>(Lifetime.Singleton);
            
            builder.RegisterInstance(_webBridge);
        }
    }
}