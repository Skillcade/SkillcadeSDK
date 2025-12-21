namespace SkillcadeSDK.Replays.Events
{
    public class ObjectCreatedEvent : ReplayEvent
    {
        public override int Size => sizeof(int) * 2;

        public int ObjectId;
        public int PrefabId;

        public ObjectCreatedEvent(int objectId, int prefabId)
        {
            ObjectId = objectId;
            PrefabId = prefabId;
        }

        public override void Handle()
        {
            // TODO: create object
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