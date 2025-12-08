using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SkillcadeSDK.ServerValidation
{
#if UNITY_SERVER
    public class ServerPayloadController : IInitializable
    {
        public ServerPayload Payload { get; private set; }

        [Inject] private readonly WebBridge _webBridge;
        
        public void Initialize()
        {
            if (_webBridge.UsePayload)
            {
                ReadPayload();
                ProcessPayload();
            }
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
                    if (fieldValue == null) continue;

                    if (field.FieldType == typeof(string))
                    {
                        field.SetValue(payload, fieldValue);
                        Debug.Log($"[ServerPayloadController] Got string variable {attribute.Name} - {fieldValue}");
                    }
                    else
                    {
                        var value = JsonConvert.DeserializeObject(fieldValue, field.FieldType);
                        field.SetValue(payload, value);
                        Debug.Log($"[ServerPayloadController] Got {field.FieldType.Name} variable {attribute.Name} - {value}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ServerPayloadController] Error on reading variable {attribute.Name}: {e}");
                }
            }
            
            Payload = payload;
        }

        private void ProcessPayload()
        {
            Payload.PublicKeyBytes = Convert.FromBase64String(Payload.SessionPublicKey);
            EnsureEd25519PublicKey(Payload.PublicKeyBytes);
            
            var (payloadBytes, signatureBytes) = DecodeToken(Payload.ServerAuthToken);
            var payloadJson = Encoding.UTF8.GetString(payloadBytes);
            var tokenPayload = JsonConvert.DeserializeObject<SessionTokenPayload>(payloadJson)
                          ?? throw new InvalidOperationException("Unable to parse join token payload.");
            
            Payload.ServerTokenPayload = tokenPayload;
            
            if (Payload.SessionExpiresAt <= DateTime.UtcNow)
                throw new InvalidOperationException("Join token or session has expired.");
        }
        
        public (byte[] Payload, byte[] Signature) DecodeToken(string token)
        {
            var parts = token.Split('.', 2);
            if (parts.Length != 2)
            {
                throw new InvalidOperationException("Join token format is invalid.");
            }

            try
            {
                var payload = Convert.FromBase64String(parts[0]);
                var signature = Convert.FromBase64String(parts[1]);
                return (payload, signature);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("Join token parts are not valid Base64.", ex);
            }
        }

        private static void EnsureEd25519PublicKey(byte[] publicKeyBytes)
        {
            // .NET Ed25519 requires a 32-byte public key.
            if (publicKeyBytes.Length != 32)
            {
                throw new InvalidOperationException("Session public key is not a valid Ed25519 key.");
            }
        }
    }
#endif
}