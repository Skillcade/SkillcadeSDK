#if UNITY_SERVER
using System;
using System.Collections.Generic;
using Aws.GameLift.Server;
using Aws.GameLift.Server.Model;
using SkillcadeSDK.Connection;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SkillcadeSDK.GameLift
{
    public class GameLiftInitializer : IInitializable, IDisposable
    {
        [Inject] private readonly IConnectionController _connectionController;
        
        public void Initialize()
        {
            _connectionController.OnStateChanged += OnConnectionStateChanged;
        }

        private void OnConnectionStateChanged(ConnectionState state)
        {
            if (state != ConnectionState.Hosting)
                return;

            Debug.Log("[GameLiftInitializer] Server connected, initializing game lift services");
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
                _connectionController.ActiveConfig.ServerListenPort,
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
    }
}
#endif