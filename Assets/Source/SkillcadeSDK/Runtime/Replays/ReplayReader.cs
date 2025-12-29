using System.IO;

namespace SkillcadeSDK.Replays
{
    public struct ReplayReader
    {
        private readonly BinaryReader _reader;

        public ReplayReader(BinaryReader reader)
        {
            _reader = reader;
        }

        public int ReadInt() => _reader.ReadInt32();
        public long ReadLong() => _reader.ReadInt64();
        public bool ReadBool() => _reader.ReadBoolean();
        public float ReadFloat() =>  _reader.ReadSingle();
        public double ReadDouble() =>  _reader.ReadDouble();

        public void SkipBytes(int count)
        {
            _reader.BaseStream.Seek(count, SeekOrigin.Current);
        }
    }
}