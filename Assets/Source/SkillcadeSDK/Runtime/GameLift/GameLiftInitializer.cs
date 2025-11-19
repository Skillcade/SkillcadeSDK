#if UNITY_SERVER
using System;
using System.Collections.Generic;
using Aws.GameLift.Server;
using Aws.GameLift.Server.Model;
using VContainer;
using VContainer.Unity;
#endif

using SkillcadeSDK.Connection;
using UnityEngine;

namespace SkillcadeSDK.GameLift
{
    public class GameLiftInitializer : MonoBehaviour
    {
        [SerializeField] private ConnectionConfig _connectionConfig;
#if UNITY_SERVER
        private void Start()
        {
            InitializeGameLiftServer();
        }

        private void InitializeGameLiftServer()
        {
            var initSdkResult = GameLiftServerAPI.InitSDK();
            if (!initSdkResult.Success)
            {
                Debug.LogError($"[GameLiftInitializer] Error initializing game lift: {initSdkResult.Error}");
                return;
            }

            Debug.Log("[GameLiftInitializer] Sdk initialized");
            
            var logFileNames = new List<string>
            {
                "Local/game/logs/serverLog.txt"
            };
            var processParameters = new ProcessParameters(OnStartSession,
                OnUpdateSession,
                OnProcessTerminate,
                OnHealthCheck,
                _connectionConfig.ServerListenPort,
                new LogParameters(logFileNames));
            
            var processReadyResult = GameLiftServerAPI.ProcessReady(processParameters);
            if (!processReadyResult.Success)
            {
                Debug.LogError($"[GameLiftInitializer] Error setting process ready: {processReadyResult.Error}");
                return;
            }

            Debug.Log("[GameLiftInitializer] Server process is ready!");
        }

        private void OnStartSession(GameSession gameSession)
        {
            GameLiftServerAPI.ActivateGameSession();
        }

        private void OnUpdateSession(UpdateGameSession updateGameSession)
        {
        }

        private void OnProcessTerminate()
        {
            GameLiftServerAPI.ProcessEnding();
        }

        private bool OnHealthCheck()
        {
            return true;
        }

        public void Dispose()
        {
            GameLiftServerAPI.Destroy();
        }
#endif
    }
}