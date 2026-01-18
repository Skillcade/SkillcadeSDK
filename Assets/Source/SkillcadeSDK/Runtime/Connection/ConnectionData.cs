namespace SkillcadeSDK.Connection
{
    public class ConnectionData
    {
        public string ServerAddress;
        public ushort ServerListenPort;
        public ushort WssConnectPort;
        public string WssServerName;
        public bool UseEncryption;

        public float ReconnectDelay;
        public int ReconnectAttempts;
        
        public int TargetPlayerCount;
    }
}