using System;
using System.Collections.Generic;
using System.IO;
using SkillcadeSDK.Replays.Components;
using UnityEngine;
using VContainer.Unity;

namespace SkillcadeSDK.Replays
{
    // Inherit from this class in framework implementation to use framework time service
    // Call OnNetworkTick on every network tick if the game is running
    // Call StartWrite and FinishWrite on start and finish active game state
    public abstract class ReplayService : IInitializable, IDisposable
    {
        private const string FileName = "replay.txt";
        
        private readonly List<ReplayObjectHandler> _activeObjects = new();
        private readonly List<byte[]> _replayData = new();

        public virtual void Initialize()
        {
        }

        public virtual void Dispose()
        {
        }

        protected void StartWrite()
        {
            _replayData.Clear();
            GC.Collect();
        }

        protected void FinishWrite()
        {
            var filePath = Path.Combine(Application.streamingAssetsPath, FileName);
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate);
            foreach (var frameData in _replayData)
            {
                stream.Write(frameData, 0, frameData.Length);
            }

            Debug.Log($"[ReplayService] Replay for {_replayData.Count} frames was written to {filePath}");
            _replayData.Clear();
            GC.Collect();
        }

        protected void OnNetworkTick(int tick)
        {
            using var stream = new MemoryStream();
            using var binaryWriter = new BinaryWriter(stream);
            
            var writer = new ReplayWriter(binaryWriter);
            writer.WriteInt(tick);
            writer.WriteInt(_activeObjects.Count);

            foreach (var objectHandler in _activeObjects)
            {
                objectHandler.Write(writer);
            }

            var frameData = stream.ToArray();
            _replayData.Add(frameData);
            Debug.Log($"[ReplayService] Frame {tick} read {frameData.Length} bytes from {_activeObjects.Count} objects");
        }

        public void RegisterObjectHandler(ReplayObjectHandler handler)
        {
            _activeObjects.Add(handler);
        }

        public void UnregisterObjectHandler(ReplayObjectHandler handler)
        {
            _activeObjects.Remove(handler);
        }
    }
}