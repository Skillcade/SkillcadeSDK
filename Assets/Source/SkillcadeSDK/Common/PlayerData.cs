namespace SkillcadeSDK.Common
{
    public struct PlayerData
    {
        public int NetworkPlayerId;
        public string UserId;
        public string MatchId;
        public string Nickname;
        public int Ping;
        public bool IsReady;
        public bool InGame;

        public PlayerData WithNickname(string nickname)
        {
            Nickname = nickname;
            return this;
        }

        public PlayerData WithPing(int ping)
        {
            Ping = ping;
            return this;
        }

        public PlayerData WithUserId(string userId)
        {
            UserId = userId;
            return this;
        }

        public PlayerData WithMatchId(string matchId)
        {
            MatchId = matchId;
            return this;
        }

        public PlayerData WithIsReady(bool isReady)
        {
            IsReady = isReady;
            return this;
        }

        public PlayerData WithInGame(bool inGame)
        {
            InGame = inGame;
            return this;
        }

        public override string ToString()
        {
            return $"[{Nickname} - {NetworkPlayerId}] ping: {Ping}ms, isReady: {IsReady}, matchId: {MatchId}, userId: {UserId}";
        }
    }
}