using UnityEngine;

namespace SkillcadeSDK.Common.Players
{
    public interface IPlayerData
    {
        public int PlayerNetworkId { get; set; }
        
        public bool TryGetData<T>(string key, out T data);
        public bool TryGetPlayerObject<T>(out T playerObject) where T : MonoBehaviour;
    }
}