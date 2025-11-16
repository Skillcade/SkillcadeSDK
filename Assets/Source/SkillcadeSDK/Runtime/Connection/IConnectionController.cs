using System;

namespace SkillcadeSDK.Connection
{
    public interface IConnectionController
    {
        event Action<ConnectionState> OnStateChanged;
        event Action<DisconnectionReason> OnDisconnected;
        
        ConnectionState ConnectionState { get; }
        ConnectionData ActiveConfig { get; }

        void StartServer(ConnectionData config);
        void StartClient(ConnectionData config);
        void Disconnect();
    }
}