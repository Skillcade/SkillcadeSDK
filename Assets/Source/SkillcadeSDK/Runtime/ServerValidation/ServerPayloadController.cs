using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VContainer.Unity;

namespace SkillcadeSDK.ServerValidation
{
#if UNITY_SERVER
    public class ServerPayloadController : IInitializable
    {
        public ServerPayload Payload { get; private set; }
        
        public void Initialize()
        {
            ReadPayload();
        }
        
        private void ReadPayload()
        {
            var payload = new ServerPayload();
            var fields = typeof(ServerPayload).GetFields().Where(x => x.GetCustomAttribute(typeof(ServerPayloadVariableAttribute)) != null);
            
            foreach (var field in fields)
            {
                var attribute = field.GetCustomAttribute<ServerPayloadVariableAttribute>();
                try
                {
                    var fieldValue = Environment.GetEnvironmentVariable(attribute.Name);
                    if (fieldValue != null)
                        field.SetValue(payload, fieldValue);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ServerPayloadController] Error on reading variable {attribute.Name}: {e}");
                }
            }
            
            Payload = payload;
        }
    }
#endif
}