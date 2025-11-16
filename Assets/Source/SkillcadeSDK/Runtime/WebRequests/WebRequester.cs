using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SkillcadeSDK.Connection;
using UnityEngine;
using VContainer;

namespace SkillcadeSDK.WebRequests
{
    public class WebRequester
    {
        private const string BaseUri = "https://demo.skillcade.com";
        private const string MediaTypeJson = "application/json";

        [Inject] private readonly IConnectionController _connectionController;

        public async Task SendWinner(string matchId, string winnerId)
        {
            if (_connectionController.ConnectionState != ConnectionState.Hosting)
                return;
            
            if (string.IsNullOrEmpty(matchId) || string.IsNullOrEmpty(winnerId))
            {
                Debug.LogError("[WebRequester] Match id and winner id are empty");
                return;
            }

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUri)
            };

            var request = new ChooseWinnerRequest
            {
                WinnerId = winnerId
            };

            Debug.Log($"[WebRequester] Sending winner request, match id: {matchId}, winnerId: {winnerId}");
            
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
}