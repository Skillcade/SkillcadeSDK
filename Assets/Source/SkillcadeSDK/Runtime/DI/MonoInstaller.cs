using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SkillcadeSDK.DI
{
    public abstract class MonoInstaller : MonoBehaviour, IInstaller
    {
        public abstract void Install(IContainerBuilder builder);
    }
}