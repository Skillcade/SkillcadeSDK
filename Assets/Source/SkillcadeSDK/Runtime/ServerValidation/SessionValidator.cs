#if UNITY_SERVER
using System;
using System.Text;
using Chaos.NaCl;
using Newtonsoft.Json;
using VContainer;

namespace SkillcadeSDK.ServerValidation
{
    public class SessionValidator
    {
        [Inject] private readonly ServerPayloadController _serverPayloadController;
        
        public SessionTokenPayload ValidateToken(string token)
        {
            var (payloadBytes, signatureBytes) = _serverPayloadController.DecodeToken(token);
            if (!Ed25519.Verify(signatureBytes, payloadBytes, _serverPayloadController.Payload.PublicKeyBytes))
                throw new InvalidOperationException("Invalid join token signature.");

            var payloadJson = Encoding.UTF8.GetString(payloadBytes);
            var payload = JsonConvert.DeserializeObject<SessionTokenPayload>(payloadJson)
                          ?? throw new InvalidOperationException("Unable to parse join token payload.");

            if (!string.Equals(payload.GameSessionId, _serverPayloadController.Payload.ServerTokenPayload.GameSessionId, StringComparison.Ordinal))
                throw new InvalidOperationException("Join token was issued for a different session.");

            if (payload.ExpiresAtUtc <= DateTime.UtcNow)
                throw new InvalidOperationException("Join token or session has expired.");

            return payload;
        }
    }
}
#endif