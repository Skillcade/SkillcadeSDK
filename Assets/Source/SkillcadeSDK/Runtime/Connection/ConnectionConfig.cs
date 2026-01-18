using UnityEngine;

namespace SkillcadeSDK.Connection
{
    [CreateAssetMenu(fileName = "Connection Config", menuName = "Configs/Connection")]
    public class ConnectionConfig : ScriptableObject
    {
        [SerializeField] public string ServerAddress;
        [SerializeField] public ushort ServerListenPort;
        [SerializeField] public ushort WssConnectPort;
        [SerializeField] public string WssServerName;
        [SerializeField] public bool UseEncryption;
        [SerializeField] public bool SkillcadeHubIntegrated;

        [Header("Reconnect")]
        [SerializeField] public float ReconnectDelay;
        [SerializeField] public int ReconnectAttempts;

        [Header("Game")]
        [SerializeField] public int TargetPlayerCount;

        public ConnectionData GetData()
        {
            return new ConnectionData
            {
                ServerAddress = ServerAddress,
                ServerListenPort = ServerListenPort,
                WssServerName = WssServerName,
                WssConnectPort = WssConnectPort,
                UseEncryption = UseEncryption,
                ReconnectDelay = ReconnectDelay,
                ReconnectAttempts = ReconnectAttempts,
                TargetPlayerCount = TargetPlayerCount,
            };
        }
    }
}