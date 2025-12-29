using System;
using System.Collections.Generic;
using System.IO;
using SkillcadeSDK.Replays.Components;
using SkillcadeSDK.Replays.Events;
using UnityEngine;

namespace SkillcadeSDK.Replays
{
    // Inherit from this class in framework implementation to use framework time service
    // Call OnNetworkTick on every network tick if the game is running
    // Call StartWrite and FinishWrite on start and finish active game state
    public abstract class ReplayWriteService
    {
        private const string FileName = "replay.txt";
        
        private readonly List<ReplayObjectHandler> _activeObjects = new();
        private readonly List<byte[]> _replayData = new();

        private readonly List<ReplayEvent> _pendingEvents = new();

        private bool _active;

        public void AddEvent(ReplayEvent replayEvent)
        {
            _pendingEvents.Add(replayEvent);
        }

        public void RegisterObjectHandler(ReplayObjectHandler handler)
        {
            _activeObjects.Add(handler);
            AddEvent(new ObjectCreatedEvent(handler.ObjectId, handler.PrefabId));
            Debug.Log($"[ReplayService] Register object {handler.ObjectId} with prefab {handler.PrefabId}");
        }

        public void UnregisterObjectHandler(ReplayObjectHandler handler)
        {
            _activeObjects.Remove(handler);
            AddEvent(new ObjectDestroyedEvent(handler.ObjectId, handler.PrefabId));
            Debug.Log($"[ReplayService] Unregister object {handler.ObjectId} with prefab {handler.PrefabId}");
        }

        protected void StartWrite()
        {
            ReplayDataObjectsRegistry.CollectDataObjectTypes();
            _active = true;
            _replayData.Clear();
            GC.Collect();
        }

        protected void FinishWrite()
        {
            _active = false;
            
            var filePath = Path.Combine(Application.streamingAssetsPath, FileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            var writer = new BinaryWriter(stream);
            writer.Write(_replayData.Count);

            for (var i = 0; i < _replayData.Count; i++)
            {
                var frameData = _replayData[i];
                Debug.Log($"[ReplayWriteService] frame {i} has {frameData.Length} bytes");
                writer.Write(frameData.Length);
                writer.Write(frameData);
            }

            Debug.Log($"[ReplayService] Replay for {_replayData.Count} frames was written to {filePath}");
            _replayData.Clear();
            GC.Collect();
        }

        protected void OnNetworkTick(int tick)
        {
            if (!_active) return;
            
            using var stream = new MemoryStream();
            using var binaryWriter = new BinaryWriter(stream);
            
            var writer = new ReplayWriter(binaryWriter);
            writer.WriteInt(tick);

            int eventsSize = sizeof(int);
            foreach (var pendingEvent in _pendingEvents)
            {
                eventsSize += writer.GetSize(pendingEvent);
            }

            writer.WriteInt(eventsSize);
            writer.WriteInt(_pendingEvents.Count);

            Debug.Log($"[ReplayWriteService] Frame {tick} write {_pendingEvents.Count} events with size {eventsSize}");
            
            foreach (var pendingEvent in _pendingEvents)
            {
                writer.Write(pendingEvent);
            }
            
            _pendingEvents.Clear();
            
            int objectsSize = sizeof(int);
            foreach (var objectHandler in _activeObjects)
            {
                objectsSize += objectHandler.GetSize(writer);
            }
            
            writer.WriteInt(objectsSize);
            writer.WriteInt(_activeObjects.Count);
            
            Debug.Log($"[ReplayWriteService] Frame {tick} write {_activeObjects.Count} objects with size {objectsSize}");
            foreach (var objectHandler in _activeObjects)
            {
                writer.WriteInt(objectHandler.PrefabId);
                writer.WriteInt(objectHandler.ObjectId);
                objectHandler.Write(writer);
            }

            var frameData = stream.ToArray();
            _replayData.Add(frameData);
            Debug.Log($"[ReplayService] Frame {tick} read {frameData.Length} bytes from {_activeObjects.Count} objects");
        }
    }
}