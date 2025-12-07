#if UNITY_SERVER
using System;
using System.Text;
using Chaos.NaCl;
using Newtonsoft.Json;

namespace SkillcadeSDK.ServerValidation
{
    public class SessionValidator
    {
        public SessionTokenPayload ValidateToken(
            string token,
            string sessionPublicKeyBase64,
            string expectedSessionId,
            DateTime sessionExpiresAtUtc)
        {
            var (payloadBytes, signatureBytes) = DecodeToken(token);

            var publicKeyBytes = Convert.FromBase64String(sessionPublicKeyBase64);
            EnsureEd25519PublicKey(publicKeyBytes);

            if (!Ed25519.Verify(signatureBytes, payloadBytes, publicKeyBytes))
            {
                throw new InvalidOperationException("Invalid join token signature.");
            }

            var payloadJson = Encoding.UTF8.GetString(payloadBytes);
            var payload = JsonConvert.DeserializeObject<SessionTokenPayload>(payloadJson)
                          ?? throw new InvalidOperationException("Unable to parse join token payload.");

            if (!string.Equals(payload.GameSessionId, expectedSessionId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Join token was issued for a different session.");
            }

            var now = DateTime.UtcNow;
            if (payload.ExpiresAtUtc <= now || sessionExpiresAtUtc <= now)
            {
                throw new InvalidOperationException("Join token or session has expired.");
            }

            return payload;
        }

        private static (byte[] Payload, byte[] Signature) DecodeToken(string token)
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
}
#endif