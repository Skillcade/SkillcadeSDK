using System.Collections.Generic;
using UnityEngine;

namespace SkillcadeSDK.Common.Players
{
    public interface IPlayerData
    {
        public int PlayerNetworkId { get; set; }
        
        public void SetDataOnServer<T>(string key, T data);
        public void SetDataOnLocalClient<T>(string key, T data);
        public bool TryGetData<T>(string key, out T data);
        
        public void AddPlayerObject<T>(T instance) where T : MonoBehaviour;
        public void RemovePlayerObject<T>(T instance) where T : MonoBehaviour;
        public bool TryGetPlayerObject<T>(out T playerObject) where T : MonoBehaviour;
        public IEnumerator<T> GetAllPlayerObjects<T>() where T : MonoBehaviour;
    }
}