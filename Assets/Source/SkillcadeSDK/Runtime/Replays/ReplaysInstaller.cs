using SkillcadeSDK.DI;
using UnityEngine;
using VContainer;

namespace SkillcadeSDK.Replays
{
    public class ReplaysInstaller : MonoInstaller
    {
        [SerializeField] private ReplayPrefabRegistry _replayPrefabRegistry;
        [SerializeField] private ReplayReadService _replayReadService;
        
        public override void Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_replayPrefabRegistry);
            builder.RegisterInstance(_replayReadService);
        }
    }
}