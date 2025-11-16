namespace SkillcadeSDK.StateMachine
{
    public interface INetworkStateMachineSyncer
    {
        public delegate void OnStateChanged(StateData prev, StateData next);
        
        public event OnStateChanged StateChanged;

        public bool IsServer { get; }
        public bool IsClient { get; }
        public StateData CurrentState { get; }

        public void SetStateOnServer(StateData stateData);
    }
}