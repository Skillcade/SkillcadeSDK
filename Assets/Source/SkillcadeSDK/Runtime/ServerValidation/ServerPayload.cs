using System;

namespace SkillcadeSDK.ServerValidation
{
#if UNITY_SERVER
    public class ServerPayload
    {
        [ServerPayloadVariable("MATCH_ID")]
        public string MatchId;
        
        [ServerPayloadVariable("BACKEND_AUTH_TOKEN")]
        public string ServerAuthToken;
    }
#endif
}