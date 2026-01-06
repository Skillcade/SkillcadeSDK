using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using VContainer;

namespace SkillcadeSDK.Replays
{
    public class ReplayReadService : MonoBehaviour
    {
        public const int ServerWorldId = 0;

        public event Action OnWorldChanged;
        
        public bool IsPlaying { get; set; }
        public bool IsReplayReady { get; set; }
        public ReplayInfo ReplayInfo { get; private set; }
        
        public float CurrentTime => _currentTime;
        public float TotalTime => _totalTime;
        public float TickInterval => _frameInterval;
        
        public int CurrentActiveWorldId { get; private set; }
        public ReplayClientWorld CurrentActiveWorld => _clientWorlds.ContainsKey(CurrentActiveWorldId) ? _clientWorlds[CurrentActiveWorldId] : null;
        public IReadOnlyDictionary<int, ReplayClientWorld> ClientWorlds => _clientWorlds;

        [SerializeField] private float _currentTime;
        [SerializeField] private float _totalTime;
        [SerializeField] private float _timeScale;
        [SerializeField] private int _tickRate;
        [SerializeField] private float _frameTimer;
        [SerializeField] private float _frameInterval;
        [SerializeField] private int _currentFrameId;
        [SerializeField] private int _maxFramesCount;

        [Inject] private readonly IObjectResolver _objectResolver;
        
        private Dictionary<int, ReplayClientWorld> _clientWorlds;
        
        public void ReadReplay(ReplayFileResult fileResult)
        {
            ReplayDataObjectsRegistry.CollectDataObjectTypes();
            if (!TryReadFile(fileResult))
                return;
            
            _frameTimer = 0f;
            _frameInterval = 1.0f / _tickRate;
            _currentTime = 0f;
            _maxFramesCount = _clientWorlds.Select(x => x.Value.Frames.Count).Max();
            _totalTime = _maxFramesCount * _frameInterval;

            if (_totalTime <= 0)
            {
                IsReplayReady = false;
                Debug.LogError("[ReplayReadService] Total replay time is 0");
                return;
            }
            
            _currentFrameId = -1;
            CurrentActiveWorldId = -1;
            SetActiveWorld(0);
            ReadFrame(0);
            IsReplayReady = true;
        }

        public void SetActiveWorld(int clientId)
        {
            if (!_clientWorlds.ContainsKey(clientId))
            {
                Debug.LogError($"[ReplayReadService] No world {clientId}");
                return;
            }
            
            if (CurrentActiveWorldId == clientId)
                return;

            CurrentActiveWorld?.SetWorldActive(false);
            CurrentActiveWorldId = clientId;
            CurrentActiveWorld?.SetWorldActive(true);
            OnWorldChanged?.Invoke();
        }

        private bool TryReadFile(ReplayFileResult fileResult)
        {
            if (!fileResult.IsSuccess)
                return false;

            _clientWorlds ??= new Dictionary<int, ReplayClientWorld>();
            foreach (var clientWorld in _clientWorlds)
            {
                clientWorld.Value.Dispose();
            }
            _clientWorlds.Clear();
            
            using var memoryStream = new MemoryStream(fileResult.Data);
            using var binaryReader = new BinaryReader(memoryStream);
            var reader = new ReplayReader(binaryReader);

            var infoBytesLength = reader.ReadInt();
            var infoBytes = binaryReader.ReadBytes(infoBytesLength);
            var infoJson = System.Text.Encoding.UTF8.GetString(infoBytes);

            Debug.Log($"[ReplayReadService] Got info json with length {infoBytesLength}: {infoJson}");
            try
            {
                ReplayInfo = JsonConvert.DeserializeObject<ReplayInfo>(infoJson);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ReplayReadService] Error on deserialising replay info: {e}");
                ReplayInfo = new ReplayInfo();
            }

            int clientsCount = reader.ReadInt();
            for (int i = 0; i < clientsCount; i++)
            {
                int clientId = reader.ReadInt();
                int frameCount = reader.ReadInt();
                Debug.Log($"[ReplayReadService] Read {frameCount} frames for client {clientId}");

                var frames = new List<ReplayReadFrameData>();
                for (int j = 0; j < frameCount; j++)
                {
                    int frameId = reader.ReadInt();
                    int frameSize = reader.ReadInt();
                    var frameData = binaryReader.ReadBytes(frameSize);
                    
                    frames.Add(new ReplayReadFrameData
                    {
                        FrameId = frameId,
                        Data = frameData
                    });
                }

                var world = new ReplayClientWorld(clientId, frames);
                _objectResolver.Inject(world);
                _clientWorlds.Add(clientId, world);
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

            if (_currentFrameId >= _maxFramesCount)
            {
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

            _currentFrameId = frameId;
            foreach (var clientWorld in _clientWorlds)
            {
                clientWorld.Value.ReadFrame(frameId);
            }
        }
    }
}