using System.Collections.Generic;

namespace SkillcadeSDK.Common.Players
{
    public interface IPlayersController
    {
        public delegate void PlayerDataEventHandler(int playerId, IPlayerData data);
        
        public event PlayerDataEventHandler OnPlayerAdded;
        public event PlayerDataEventHandler OnPlayerDataUpdated;
        public event PlayerDataEventHandler OnPlayerRemoved;

        public IReadOnlyDictionary<int, IPlayerData> Players { get; }
        public int LocalPlayerId { get; }

        public void SetPlayerDataOnServer<T>(int playerId, string key, T data);
        public void SetLocalPlayerDataOnClient<T>(string key, T data);
        
        public void AddPlayerObject<T>(int playerId, T networkObject);
        public void RemovePlayerObject<T>(int playerId, T networkObject);
    }
}