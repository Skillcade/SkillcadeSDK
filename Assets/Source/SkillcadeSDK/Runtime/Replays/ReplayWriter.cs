using System.IO;
using SkillcadeSDK.Replays.Components;

namespace SkillcadeSDK.Replays
{
    public struct ReplayWriter
    {
        private readonly BinaryWriter _writer;
        public ReplayWriter(BinaryWriter writer) => _writer = writer;
        
        public void Write(ReplayComponent component)
        {
            WriteInt(component.Id);
            WriteInt(component.Size);
            component.Write(this);
        }

        public void WriteInt(int value)
        {
            _writer.Write(value);
        }

        public void WriteLong(long value)
        {
            _writer.Write(value);
        }
    }
}