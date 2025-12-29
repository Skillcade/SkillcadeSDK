using UnityEngine;
using VContainer;

namespace SkillcadeSDK.Replays.Events
{
    public class ObjectCreatedEvent : ReplayEvent
    {
        public override int Size => sizeof(int) * 2;

        public int ObjectId;
        public int PrefabId;

        [Inject] private readonly ReplayReadService _replayReadService;
        [Inject] private readonly ReplayPrefabRegistry _replayPrefabRegistry;
        
        public ObjectCreatedEvent() { }

        public ObjectCreatedEvent(int objectId, int prefabId)
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
            Debug.Log($"[ObjectCreatedEvent] Handle event with object {ObjectId} and prefab {PrefabId}");
            if (!_replayPrefabRegistry.TryGetPrefab(PrefabId, out var prefab))
            {
                Debug.LogError($"[ObjectCreatedEvent] Prefab {PrefabId} not found");
                return;
            }

            var instance = prefab.Instantiate();
            instance.InitializeReplay(ObjectId);
            _replayReadService.RegisterObject(instance);
        }

        public override void Undo()
        {
            _replayReadService.DeleteObject(ObjectId, out var handler);
            if (handler != null)
                handler.DestroyGameObject();

            Debug.Log($"[ObjectCreatedEvent] Undo event with object {ObjectId} and prefab {PrefabId}, found object: {handler != null}");
        }
    }
}