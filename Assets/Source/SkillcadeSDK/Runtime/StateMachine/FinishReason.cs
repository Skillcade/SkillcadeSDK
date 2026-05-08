namespace SkillcadeSDK.StateMachine
{
    /// <summary>
    /// Reason for game finish.
    /// </summary>
    public enum FinishReason
    {
        /// <summary>
        /// A player completed game goal.
        /// </summary>
        CompletedGoal,

        /// <summary>
        /// Technical win due to other players leaving.
        /// </summary>
        TechnicalWin,

        /// <summary>
        /// Game ended in a draw (time ran out, etc.).
        /// </summary>
        Draw,
    }
}
