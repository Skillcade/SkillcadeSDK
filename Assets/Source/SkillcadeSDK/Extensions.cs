using SkillcadeSDK.Connection;
using SkillcadeSDK.DI;
using UnityEngine;
using UnityEngine.Pool;

namespace SkillcadeSDK
{
    public static class Extensions
    {
        public static void SetLayerWithChildren(this GameObject target, int layer)
        {
            target.layer = layer;
            using var childrenPooled = ListPool<Transform>.Get(out var children);
            target.GetComponentsInChildren(children);
            foreach (var child in children)
            {
                child.gameObject.layer = layer;
            }
        }

        public static void InjectToMe(this object target)
        {
            if (ContainerSingletonWrapper.Instance != null)
                ContainerSingletonWrapper.Instance.Resolver.Inject(target);
        }

        public static bool IsConnectedOrHosting(this ConnectionState connectionState) =>
            connectionState is ConnectionState.Connected or ConnectionState.Hosting;
    }
}