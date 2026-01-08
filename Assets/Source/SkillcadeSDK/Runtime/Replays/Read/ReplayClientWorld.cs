using System;
using System.Collections.Generic;
using System.IO;
using SkillcadeSDK.Replays.Components;
using SkillcadeSDK.Replays.Events;
using UnityEngine;
using VContainer;

namespace SkillcadeSDK.Replays
{
    public class ReplayClientWorld : IDisposable
    {
        public event Action OnColorChanged;
        
        public int WorldId { get; }
        public int Tick { get; private set; }
        public bool IsActive { get; private set; }
        public float Transparency { get; private set; }
        public Color Color { get; private set; }
        
        public IReadOnlyList<ReplayReadFrameData> Frames => _frames;
        public IReadOnlyDictionary<int, ReplayObjectHandler> ReplayObjects => _replayObjects;

        [Inject] private readonly IObjectResolver _objectResolver;

        private readonly List<ReplayReadFrameData> _frames;
        private readonly List<ReplayEvent> _lastFrameEvents;
        private readonly Dictionary<int, ReplayObjectHandler> _replayObjects;
        
        private int _currentFrameId;

        public ReplayClientWorld(int worldId, List<ReplayReadFrameData> frames, float transparency)
        {
            WorldId = worldId;
            _frames = frames;
            _lastFrameEvents = new List<ReplayEvent>();
            _replayObjects = new Dictionary<int, ReplayObjectHandler>();
            _currentFrameId = -1;
            Transparency = transparency;
            Color = Color.white;
        }
        
        public void RegisterObject(ReplayObjectHandler handler)
        {
            Debug.Log($"[ReplayClientWorld] Add object {handler.ObjectId} to world {handler.WorldId}");
            _replayObjects.Add(handler.ObjectId, handler);
            handler.SetVisible(IsActive ? 1f : Transparency);
        }
        
        public void DeleteObject(int id, out ReplayObjectHandler handler)
        {
            _replayObjects.Remove(id, out handler);
            Debug.Log($"[ReplayClientWorld] Remove object {handler.ObjectId} from world {handler.WorldId}");
        }

        public void SetWorldActive(bool value)
        {
            IsActive = value;
            UpdateObjectsVisibility();
        }

        public void SetWorldTransparency(float value)
        {
            Transparency = value;
            UpdateObjectsVisibility();
        }

        public void SetWorldColor(Color value)
        {
            Color = value;
            OnColorChanged?.Invoke();
        }

        private void UpdateObjectsVisibility()
        {
            foreach (var handler in _replayObjects)
            {
                handler.Value.SetVisible(IsActive ? 1f : Transparency);
            }
        }

        public void Dispose()
        {
            CleanupObjects();
        }
        
        private void CleanupObjects()
        {
            foreach (var replayObject in _replayObjects)
            {
                replayObject.Value.DestroyGameObject();
            }
            
            _replayObjects.Clear();
        }
        
        public void ReadFrame(int frameId)
        {
            if (_currentFrameId == frameId)
                return;
            
            if (frameId >= _frames.Count)
                return;

            bool isMovingBakwards = frameId < _currentFrameId;
            if (isMovingBakwards)
            {
                foreach (var lastFrameEvent in _lastFrameEvents)
                {
                    lastFrameEvent.Undo(WorldId);
                }
            }

            _currentFrameId = frameId;
            _lastFrameEvents.Clear();
            
            var frame = _frames[frameId];
            
            using var stream = new MemoryStream(frame.Data);
            using var binaryReader = new BinaryReader(stream);
            var reader = new ReplayReader(binaryReader);

            Tick = reader.ReadInt();
            
            int eventsCount = reader.ReadInt();
            for (int j = 0; j < eventsCount; j++)
            {
                int id = reader.ReadUshort();
                int size = reader.ReadUshort();
                if (!ReplayDataObjectsRegistry.IdToType.TryGetValue(id, out var type))
                {
                    Debug.LogError($"[ReplayReadService] Can't get event type for id {id}");
                    reader.SkipBytes(size);
                    continue;
                }

                var eventInstance = Activator.CreateInstance(type) as ReplayEvent;
                if (eventInstance == null)
                {
                    Debug.LogError($"[ReplayReadService] Wrong event type while reading events: {type.Name}");
                    reader.SkipBytes(size);
                    continue;
                }
                
                _objectResolver.Inject(eventInstance);
                eventInstance.Read(reader);
                _lastFrameEvents.Add(eventInstance);
                
                if (!isMovingBakwards)
                    eventInstance.Handle(WorldId);
            }

            int objectsCount = reader.ReadInt();
            for (int j = 0; j < objectsCount; j++)
            {
                int prefabId = reader.ReadInt();
                int objectId = reader.ReadInt();
                if (!_replayObjects.TryGetValue(objectId, out var handler))
                {
                    Debug.LogError($"[ReplayReadService] Object {objectId} not found");
                    int componentsCount = reader.ReadUshort();
                
                    Debug.Log($"[ReplayReadService] Got object {objectId} with prefab {prefabId} and {componentsCount} components");
                    for (int k = 0; k < componentsCount; k++)
                    {
                        int id = reader.ReadUshort();
                        int size = reader.ReadUshort();
                        Debug.Log($"[ReplayReadService] Got component {id} with size {size}");
                        reader.SkipBytes(size);
                    
                        if (ReplayDataObjectsRegistry.IdToType.TryGetValue(id, out var type))
                            Debug.Log($"[ReplayReadService] Component type is {type.Name}");
                        else
                            Debug.LogError($"[ReplayReadService] Can't get component type for id {id}");
                    }
                    continue;
                }
                
                handler.Read(reader);
            }
        }
    }
}