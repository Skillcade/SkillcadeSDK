namespace SkillcadeSDK.Replays.Events
{
    public abstract class ReplayEvent : IReplayDataObject
    {
        public abstract int Size { get; }

        public abstract void Handle();
        public abstract void Read(ReplayReader reader);
        public abstract void Write(ReplayWriter writer);
    }
}