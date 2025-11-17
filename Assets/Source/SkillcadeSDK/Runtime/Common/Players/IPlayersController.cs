using System.Collections.Generic;

namespace SkillcadeSDK.Common.Players
{
    public interface IPlayersController
    {
        public delegate void PlayerDataEventHandler(int playerId, IPlayerData data);
        
        public event PlayerDataEventHandler OnPlayerAdded;
        public event PlayerDataEventHandler OnPlayerDataUpdated;
        public event PlayerDataEventHandler OnPlayerRemoved;
        
        public int LocalPlayerId { get; }
        
        public bool TryGetPlayerData(int playerId, out IPlayerData data);
        public IEnumerable<IPlayerData> GetAllPlayersData();
    }
}