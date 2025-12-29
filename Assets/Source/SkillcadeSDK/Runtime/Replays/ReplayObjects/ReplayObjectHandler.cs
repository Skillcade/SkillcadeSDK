using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace SkillcadeSDK.Replays.Components
{
    public abstract class ReplayObjectHandler : MonoBehaviour
    {
        public int PrefabId => _prefabId;
        public int ObjectId => _isReplaying ? _replayObjectId : RuntimeNetworkObjectId;
        
        protected abstract int RuntimeNetworkObjectId { get; }

        private bool _isReplaying;
        private int _replayObjectId;

        [SerializeField] private int _prefabId;
        [SerializeField] private GameObject _targetObject;

        [Inject] private readonly ReplayWriteService _replayWriteService;
        
        private Dictionary<Type, IReplayComponent> _replayComponents;

        protected virtual void Awake()
        {
            var replayComponents = GetComponentsInChildren<IReplayComponent>();
            _replayComponents = new Dictionary<Type, IReplayComponent>();
            foreach (var component in replayComponents)
            {
                _replayComponents.Add(component.GetType(), component);
            }
        }

        public void InitializeReplay(int objectId)
        {
            _isReplaying = true;
            _replayObjectId = objectId;
        }

        protected void Register()
        {
            this.InjectToMe();
            if (!_isReplaying)
                _replayWriteService.RegisterObjectHandler(this);
        }

        protected void Unregister()
        {
            if (!_isReplaying)
                _replayWriteService.UnregisterObjectHandler(this);
        }

        public int GetSize(ReplayWriter writer)
        {
            int size = sizeof(int) * 3;
            foreach (var component in _replayComponents.Values)
            {
                size += writer.GetSize(component);
            }
            
            return size;
        }

        public void Write(ReplayWriter writer)
        {
            writer.WriteInt(_replayComponents.Count);
            foreach (var component in _replayComponents.Values)
            {
                writer.Write(component);
            }
        }

        public void Read(ReplayReader reader)
        {
            _targetObject.SetActive(true);
            int componentsCount = reader.ReadInt();
            for (int i = 0; i < componentsCount; i++)
            {
                int id = reader.ReadInt();
                int size = reader.ReadInt();
                if (!ReplayDataObjectsRegistry.IdToType.TryGetValue(id, out var type))
                {
                    Debug.LogError($"[ReplayObjectHandler] Can't get replay component type for id {id}");
                    reader.SkipBytes(size);
                    continue;
                }

                if (!_replayComponents.TryGetValue(type, out var component))
                {
                    Debug.LogError($"[ReplayObjectHandler] Component with type {type.Name} not registered!");
                    reader.SkipBytes(size);
                    continue;
                }
                
                component.Read(reader);
            }
        }
    }
}