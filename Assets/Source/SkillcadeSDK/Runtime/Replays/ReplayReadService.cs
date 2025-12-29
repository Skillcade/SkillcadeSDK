using System;
using System.Collections.Generic;
using System.IO;
using SkillcadeSDK.Replays.Components;
using SkillcadeSDK.Replays.Events;
using UnityEngine;
using VContainer;

namespace SkillcadeSDK.Replays
{
    public class ReplayReadService : MonoBehaviour
    {
        private class FrameData
        {
            public int Tick;
            public byte[] Data;
        }
        
        public bool IsPlaying { get; set; }
        public bool IsReplayReady { get; set; }
        public int Tick { get; private set; }
        
        public float CurrentTime => _currentTime;
        public float TotalTime => _totalTime;

        [SerializeField] private float _currentTime;
        [SerializeField] private float _totalTime;
        [SerializeField] private float _timeScale;
        [SerializeField] private int _tickRate;
        [SerializeField] private float _frameTimer;
        [SerializeField] private float _frameInterval;
        [SerializeField] private int _currentFrameId;

        [Inject] private readonly IObjectResolver _objectResolver;
        
        private List<FrameData> _frames;
        private List<ReplayEvent> _lastFrameEvents;
        private Dictionary<int, ReplayObjectHandler> _replayObjects;
        
        public void RegisterObject(ReplayObjectHandler handler) => _replayObjects.Add(handler.ObjectId, handler);
        public void DeleteObject(int id, out ReplayObjectHandler handler) => _replayObjects.Remove(id, out handler);
        
        public void ReadReplay(ReplayFileResult fileResult)
        {
            ReplayDataObjectsRegistry.CollectDataObjectTypes();
            if (!TryReadFile(fileResult))
                return;
            
            _frameTimer = 0f;
            _frameInterval = 1.0f / _tickRate;
            _currentTime = 0f;
            _totalTime = _frames.Count * _frameInterval;

            if (_totalTime <= 0)
            {
                IsReplayReady = false;
                Debug.LogError("[ReplayReadService] Total replay time is 0");
                return;
            }

            _lastFrameEvents ??= new List<ReplayEvent>();
            _lastFrameEvents.Clear();
            
            _replayObjects ??= new Dictionary<int, ReplayObjectHandler>();
            CleanupObjects();

            _currentFrameId = -1;
            ReadFrame(0);
            IsReplayReady = true;
        }

        private void CleanupObjects()
        {
            foreach (var replayObject in _replayObjects)
            {
                replayObject.Value.DestroyGameObject();
            }
            
            _replayObjects.Clear();
        }

        private bool TryReadFile(ReplayFileResult fileResult)
        {
            if (!fileResult.IsSuccess)
                return false;
            
            _frames ??= new List<FrameData>();
            _frames.Clear();
            
            using var memoryStream = new MemoryStream(fileResult.Data);
            using var binaryReader = new BinaryReader(memoryStream);
            var reader = new ReplayReader(binaryReader);
            
            int frameCount = reader.ReadInt();
            Debug.Log($"[ReplayReadService] Read {frameCount} frames");
            
            for (int i = 0; i < frameCount; i++)
            {
                int frameSize = reader.ReadInt();
                int tick = reader.ReadInt();
                frameSize -= sizeof(int);
                var frameData = binaryReader.ReadBytes(frameSize);
                _frames.Add(new FrameData
                {
                    Tick = tick,
                    Data = frameData
                });
            }
            
            return true;
        }

        public void SetNormalizedTime(float value)
        {
            value = Mathf.Clamp01(value);
            _currentTime = Mathf.Lerp(0, _totalTime, value);
            _frameTimer = _currentTime % _frameInterval;
            int targetFrameId = Mathf.FloorToInt((_currentTime - _frameTimer) / _frameInterval);
            
            if (targetFrameId > _currentFrameId)
            {
                for (int i = _currentFrameId + 1; i <= targetFrameId; i++)
                {
                    ReadFrame(i);
                }
            }
            else
            {
                for (int i = _currentFrameId - 1; i >= targetFrameId; i--)
                {
                    ReadFrame(i);
                }
            }
        }

        private void Update()
        {
            if (!IsPlaying || !IsReplayReady)
                return;

            if (_currentFrameId >= _frames.Count)
            {
                Debug.Log($"[ReplayReadService] Replay ended on frame id {_currentFrameId}, tick {Tick}");
                IsPlaying = false;
                return;
            }

            _currentTime += Time.deltaTime;
            _frameTimer += Time.deltaTime;
            while (_frameTimer >= _frameInterval)
            {
                _frameTimer -= _frameInterval;
                ReadFrame(_currentFrameId + 1);
            }
        }

        private void ReadFrame(int frameId)
        {
            if (_currentFrameId == frameId)
                return;

            bool isMovingBakwards = frameId < _currentFrameId;
            if (isMovingBakwards)
            {
                foreach (var lastFrameEvent in _lastFrameEvents)
                {
                    lastFrameEvent.Undo();
                }
            }

            _currentFrameId = frameId;
            _lastFrameEvents.Clear();
            
            var frame = _frames[frameId];
            Tick = frame.Tick;
            
            using var stream = new MemoryStream(frame.Data);
            using var binaryReader = new BinaryReader(stream);
            var reader = new ReplayReader(binaryReader);
            
            int eventsSize = reader.ReadInt();
            int eventsCount = reader.ReadInt();
            
            for (int j = 0; j < eventsCount; j++)
            {
                int id = reader.ReadInt();
                int size = reader.ReadInt();
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
                    eventInstance.Handle();
            }

            int objectsSize = reader.ReadInt();
            int objectsCount = reader.ReadInt();

            for (int j = 0; j < objectsCount; j++)
            {
                int prefabId = reader.ReadInt();
                int objectId = reader.ReadInt();
                if (!_replayObjects.TryGetValue(objectId, out var handler))
                {
                    Debug.LogError($"[ReplayReadService] Object {objectId} not found");
                    int componentsCount = reader.ReadInt();
                
                    Debug.Log($"[ReplayReadService] Got object {objectId} with prefab {prefabId} and {componentsCount} components");
                    for (int k = 0; k < componentsCount; k++)
                    {
                        int id = reader.ReadInt();
                        int size = reader.ReadInt();
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