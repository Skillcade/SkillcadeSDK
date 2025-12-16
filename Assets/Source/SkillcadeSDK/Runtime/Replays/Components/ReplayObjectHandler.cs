using UnityEngine;
using VContainer;

namespace SkillcadeSDK.Replays.Components
{
    public abstract class ReplayObjectHandler : MonoBehaviour
    {
        protected abstract int NetworkPrefabId { get; }
        protected abstract int NetworkObjectId { get; }

        [SerializeField] private ReplayComponent[] _replayComponents;

        [Inject] private readonly ReplayService _replayService;

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
    }
}