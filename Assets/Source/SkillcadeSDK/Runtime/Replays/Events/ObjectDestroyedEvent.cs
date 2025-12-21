namespace SkillcadeSDK.Replays.Events
{
    public class ObjectDestroyedEvent : ReplayEvent
    {
        public override int Size => sizeof(int) * 2;

        public int ObjectId;
        public int PrefabId;

        public ObjectDestroyedEvent(int objectId, int prefabId)
        {
            ObjectId = objectId;
            PrefabId = prefabId;
        }

        public override void Handle()
        {
            // TODO: destroy object
        }

        public override void Read(ReplayReader reader)
        {
            ObjectId = reader.ReadInt();
            PrefabId = reader.ReadInt();
        }

        public override void Write(ReplayWriter writer)
        {
            writer.WriteInt(ObjectId);
            writer.WriteInt(PrefabId);
        }
    }
}