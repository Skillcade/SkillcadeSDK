using UnityEngine;
using VContainer;

namespace SkillcadeSDK.Replays.Events
{
    public class ObjectDestroyedEvent : ReplayEvent
    {
        public override int Size => sizeof(int) * 2;

        public int ObjectId;
        public int PrefabId;
        
        [Inject] private readonly ReplayReadService _replayReadService;
        [Inject] private readonly ReplayPrefabRegistry _replayPrefabRegistry;

        public ObjectDestroyedEvent() { }
        
        public ObjectDestroyedEvent(int objectId, int prefabId)
        {
            ObjectId = objectId;
            PrefabId = prefabId;
        }

        public override void Read(ReplayReader reader)
        {
            ObjectId = reader.ReadInt();
            PrefabId = reader.ReadInt();
        }

        public override void Write(ReplayWriter writer)
        {
            writer.WriteInt(ObjectId);
            writer.WriteInt(PrefabId);
        }

        public override void Handle()
        {
            Debug.Log($"[ObjectDestroyedEvent] Handle event with object {ObjectId} and prefab {PrefabId}");
            
            _replayReadService.DeleteObject(ObjectId, out var handler);
            handler.DestroyGameObject();
        }

        public override void Undo()
        {
            if (!_replayPrefabRegistry.TryGetPrefab(PrefabId, out var prefab))
            {
                Debug.LogError($"[ObjectCreatedEvent] Prefab {PrefabId} not found");
                return;
            }

            var instance = prefab.Instantiate();
            instance.InitializeReplay(ObjectId);
            _replayReadService.RegisterObject(instance);

            Debug.Log($"[ObjectDestroyedEvent] Undo event with object {ObjectId} and prefab {PrefabId}");
        }
    }
}