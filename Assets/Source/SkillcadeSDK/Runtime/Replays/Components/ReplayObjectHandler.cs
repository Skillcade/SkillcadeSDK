using UnityEngine;
using VContainer;

namespace SkillcadeSDK.Replays.Components
{
    public abstract class ReplayObjectHandler : MonoBehaviour
    {
        public abstract int NetworkPrefabId { get; }
        public abstract int NetworkObjectId { get; }

        [Inject] private readonly ReplayService _replayService;
        
        private IReplayComponent[] _replayComponents;

        protected virtual void Awake()
        {
            _replayComponents = GetComponentsInChildren<IReplayComponent>();
            this.InjectToMe();
        }

        public void Register()
        {
            _replayService.RegisterObjectHandler(this);
        }

        public void Unregister()
        {
            _replayService.UnregisterObjectHandler(this);
        }

        public void Write(ReplayWriter writer)
        {
            writer.WriteInt(NetworkPrefabId);
            writer.WriteInt(NetworkObjectId);
            writer.WriteInt(_replayComponents.Length);

            foreach (var component in _replayComponents)
            {
                writer.Write(component);
            }
        }

        public int GetSize(ReplayWriter writer)
        {
            int size = sizeof(int) * 3;
            foreach (var component in _replayComponents)
            {
                size += writer.GetSize(component);
            }
            
            return size;
        }
    }
}