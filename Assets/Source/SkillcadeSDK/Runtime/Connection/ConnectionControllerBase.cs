using System;
using System.Collections;
using UnityEngine;
using VContainer.Unity;

namespace SkillcadeSDK.Connection
{
    public abstract class ConnectionControllerBase : MonoBehaviour, IInitializable, IConnectionController, IDisposable
    {
        public event Action<ConnectionState> OnStateChanged;
        public event Action<DisconnectionReason> OnDisconnected;
        
        public ConnectionState ConnectionState { get; private set; }
        public ConnectionData ActiveConfig { get; private set; }
        public abstract ITransportAdapter Transport { get; }

        private int _reconnectAttempts;
        private Coroutine _reconnectCoroutine;
        private WaitForSeconds _reconnectWait;

        public virtual void Initialize()
        {
            Transport.OnConnected += OnTransportConnected;
            Transport.OnDisconnected += OnTransportDisconnected;
        }

        public void StartServer(ConnectionData config)
        {
            if (ConnectionState != ConnectionState.Disconnected)
            {
                Debug.LogError($"[ConnectionControllerBase] Can't start server in state {ConnectionState}");
                return;
            }

            if (config == null)
            {
                Debug.LogError($"[ConnectionControllerBase] Can't start server cause config is null");
                return;
            }
            
            ActiveConfig = config;
            
            Debug.Log("[ConnectionControllerBase] Starting server");
            SetState(ConnectionState.Connecting);
            Transport.StartServer(config);
        }

        public void StartClient(ConnectionData config)
        {
            if (ConnectionState != ConnectionState.Disconnected)
            {
                Debug.LogError($"[ConnectionControllerBase] Can't start client in state {ConnectionState}");
                return;
            }

            if (config == null)
            {
                Debug.LogError($"[ConnectionControllerBase] Can't start client cause config is null");
                return;
            }
            
            ActiveConfig = config;

            Debug.Log("[ConnectionControllerBase] Starting client");
            SetState(ConnectionState.Connecting);
            Transport.StartClient(config);
        }

        public void Disconnect()
        {
            if (ConnectionState ==  ConnectionState.Disconnected)
                return;

            Debug.Log("[ConnectionControllerBase] Disconnecting");
            
            StopReconnect();
            
            SetState(ConnectionState.Disconnecting);
            Transport.Disconnect();
            
            OnDisconnected?.Invoke(DisconnectionReason.UserRequested);
        }

        private void OnTransportConnected()
        {
            if (Transport.IsServer)
            {
                SetState(ConnectionState.Hosting);
            }
            else if (Transport.IsClient)
            {
                _reconnectAttempts = 0;
                SetState(ConnectionState.Connected);
            }
        }

        private void OnTransportDisconnected(DisconnectionReason reason)
        {
            if (Transport.IsServer)
            {
                Debug.Log("[ConnectionControllerBase] Server stopped");
                SetState(ConnectionState.Disconnected);
            }
            else if (Transport.IsClient)
            {
                Debug.Log($"[ConnectionControllerBase] Disconnected from server: {reason}");
                OnDisconnected?.Invoke(reason);
                SetState(ConnectionState.Disconnected);
            
                if (ShouldReconnect(reason))
                    StartReconnect();
            }
        }

        private bool ShouldReconnect(DisconnectionReason reason)
        {
            if (ActiveConfig == null || !Transport.IsClient)
                return false;

            return reason is DisconnectionReason.ConnectionLost or DisconnectionReason.Timeout;
        }

        private void StartReconnect()
        {
            Debug.Log("[ConnectionControllerBase] Starting reconnect");
            if (_reconnectCoroutine != null)
            {
                Debug.Log("[ConnectionControllerBase] Reconnect already in progress");
                return;
            }

            if (_reconnectAttempts >= ActiveConfig.ReconnectAttempts)
            {
                Debug.Log("[ConnectionControllerBase] Too much reconnect attempts, can't reconnect");
                return;
            }
            
            _reconnectCoroutine = StartCoroutine(Reconnect());
        }

        private void StopReconnect()
        {
            if (_reconnectCoroutine != null)
            {
                StopCoroutine(_reconnectCoroutine);
                _reconnectCoroutine = null;
            }

            _reconnectAttempts = 0;
        }

        private IEnumerator Reconnect()
        {
            while (_reconnectAttempts < ActiveConfig.ReconnectAttempts)
            {
                _reconnectAttempts++;
                Debug.Log($"[ConnectionControllerBase] Reconnect attempt {_reconnectAttempts}/{ActiveConfig.ReconnectAttempts}");

                _reconnectWait ??= new WaitForSeconds(ActiveConfig.ReconnectDelay);
                yield return _reconnectWait;
                
                StartClient(ActiveConfig);

                while (ConnectionState == ConnectionState.Connecting)
                    yield return null;
                
                if (ConnectionState == ConnectionState.Connected)
                    break;
            }

            _reconnectCoroutine = null;
        }

        private void SetState(ConnectionState state)
        {
            if (ConnectionState == state)
                return;

            Debug.Log($"[ConnectionControllerBase] Change connection state from {ConnectionState} to {state}");
            ConnectionState = state;
            OnStateChanged?.Invoke(state);
        }

        public virtual void Dispose()
        {
            Disconnect();
        }
    }
}