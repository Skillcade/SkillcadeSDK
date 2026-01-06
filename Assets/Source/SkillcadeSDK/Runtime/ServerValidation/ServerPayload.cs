#if UNITY_SERVER || UNITY_EDITOR
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SkillcadeSDK.ServerValidation
{
    public class ServerPayload
    {
        [ServerPayloadVariable("MATCH_ID")]
        public string MatchId;
        
        [ServerPayloadVariable("BACKEND_AUTH_TOKEN")]
        public string ServerAuthToken;
        
        [ServerPayloadVariable("SESSION_PUBLIC_KEY")]
        public string SessionPublicKey;

        [ServerPayloadVariable("REPLAY_UPLOAD_URL")]
        public string ReplayUploadUrl;
        
        [ServerPayloadVariable("SESSION_EXPIRES_AT", typeof(DateTimeVariableReader))]
        public DateTime SessionExpiresAt;

        [JsonIgnore]
        public byte[] PublicKeyBytes;
    }
}
#endif