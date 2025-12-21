using System.IO;

namespace SkillcadeSDK.Replays
{
    public struct ReplayWriter
    {
        private readonly BinaryWriter _writer;
        public ReplayWriter(BinaryWriter writer) => _writer = writer;
        
        public void Write(IReplayDataObject dataObject)
        {
            var id = ReplayDataObjectsRegistry.TypeToId[dataObject.GetType()];
            WriteInt(id);
            WriteInt(dataObject.Size);
            dataObject.Write(this);
        }
        
        public int GetSize(IReplayDataObject dataObject) => dataObject.Size + sizeof(int);

        public void WriteInt(int value) => _writer.Write(value);
        public void WriteLong(long value) => _writer.Write(value);
        public void WriteBool(bool value) => _writer.Write(value);
        public void WriteFloat(float value) =>  _writer.Write(value);
        public void WriteDouble(double value) =>  _writer.Write(value);
    }
}