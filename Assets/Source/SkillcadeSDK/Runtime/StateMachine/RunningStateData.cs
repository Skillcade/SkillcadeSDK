namespace SkillcadeSDK.StateMachine
{
    /// <summary>
    /// Data passed to RunningState when entering.
    /// </summary>
    public class RunningStateData
    {
        public readonly float GameDurationSeconds;

        public RunningStateData(float gameDurationSeconds)
        {
            GameDurationSeconds = gameDurationSeconds;
        }
    }
}
