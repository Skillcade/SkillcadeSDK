using Newtonsoft.Json;

namespace SkillcadeSDK.StateMachine
{
    /// <summary>
    /// Data passed to FinishedState when entering.
    /// </summary>
    public class FinishedStateData
    {
        public readonly int WinnerClientId;
        public readonly string WinnerPlayerId;
        public readonly FinishReason FinishReason;

        public FinishedStateData(int winnerClientId, FinishReason finishReason)
            : this(winnerClientId, null, finishReason)
        {
        }

        [JsonConstructor]
        public FinishedStateData(int winnerClientId, string winnerPlayerId, FinishReason finishReason)
        {
            WinnerClientId = winnerClientId;
            WinnerPlayerId = winnerPlayerId;
            FinishReason = finishReason;
        }
    }
}
