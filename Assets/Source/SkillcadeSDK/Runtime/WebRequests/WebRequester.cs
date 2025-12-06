using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SkillcadeSDK.Connection;
using SkillcadeSDK.ServerValidation;
using UnityEngine;
using VContainer;

namespace SkillcadeSDK.WebRequests
{
#if UNITY_SERVER
    public class WebRequester
    {
        private const string BaseUri = "https://demo.skillcade.com";
        private const string MediaTypeJson = "application/json";
        private const string TokenHeaderKey = " X-Game-Server-Token";

        [Inject] private readonly IConnectionController _connectionController;
        [Inject] private readonly ServerPayloadController _serverPayloadController;

        public async Task SendWinner(string winnerId)
        {
            if (_connectionController.ConnectionState != ConnectionState.Hosting)
                return;

            if (_serverPayloadController.Payload == null)
            {
                Debug.LogError("[WebRequester] Server payload is null");
                return;
            }

            if (string.IsNullOrEmpty(_serverPayloadController.Payload.MatchId))
            {
                Debug.LogError("[WebRequester] Match id is empty");
                return;
            }

            if (string.IsNullOrEmpty(_serverPayloadController.Payload.ServerAuthToken))
            {
                Debug.LogError("[WebRequester] Server auth token is empty");
                return;
            }
            
            if (string.IsNullOrEmpty(winnerId))
            {
                Debug.LogError("[WebRequester] Winner id are empty");
                return;
            }

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUri),
                DefaultRequestHeaders = {  { TokenHeaderKey, _serverPayloadController.Payload.ServerAuthToken } }
            };

            var request = new ChooseWinnerRequest
            {
                WinnerId = winnerId
            };

            string matchId = _serverPayloadController.Payload.MatchId;
            Debug.Log($"[WebRequester] Sending winner request, match id: {matchId}, winnerId: {winnerId}, token: {_serverPayloadController.Payload.ServerAuthToken}");
            
            try
            {
                using var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, MediaTypeJson);
                using var response = await httpClient.PostAsync($"api/playing-game/{matchId}/choose-winner", jsonContent);

                Debug.Log($"[WebRequester] choose winner response status: {response.StatusCode} - {response.ReasonPhrase}");
                response.EnsureSuccessStatusCode();
                
                var responseString = await response.Content.ReadAsStringAsync();
                Debug.Log($"[WebRequester] choose winner response: {responseString}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[WebRequester] Error sending winner {e}");
            }
        }
    }
#endif
}