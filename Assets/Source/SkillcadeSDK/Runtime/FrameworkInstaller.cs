using SkillcadeSDK.Common;
using SkillcadeSDK.Common.Level;
using SkillcadeSDK.DI;
using UnityEngine;
using VContainer;

#if UNITY_SERVER
using SkillcadeSDK.ServerValidation;
using SkillcadeSDK.WebRequests;
#endif

namespace SkillcadeSDK
{
    public class FrameworkInstaller : MonoInstaller
    {
        [SerializeField] private WebBridge _webBridge;
        
        public override void Install(IContainerBuilder builder)
        {
            builder.Register<ContainerSingletonWrapper>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<LayerProvider>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<RespawnServiceProvider>(Lifetime.Singleton);

#if UNITY_SERVER
            builder.Register<WebRequester>(Lifetime.Singleton);
            builder.Register<ServerPayloadController>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
#endif
            
            builder.RegisterInstance(_webBridge);
        }
    }
}