namespace SkillcadeSDK.Connection
{
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Disconnecting,
        Failed,
        Hosting,
    }

    public enum DisconnectionReason
    {
        UserRequested,
        ConnectionLost,
        ServerStopped,
        Kicked,
        Timeout,
        Error
    }
}