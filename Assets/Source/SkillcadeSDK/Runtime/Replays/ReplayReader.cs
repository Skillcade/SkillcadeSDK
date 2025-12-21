using System.IO;

namespace SkillcadeSDK.Replays
{
    public struct ReplayReader
    {
        private BinaryReader _reader;

        public int ReadInt() => _reader.ReadInt32();
        public long ReadLong() => _reader.ReadInt64();
        public bool ReadBool() => _reader.ReadBoolean();
        public float ReadFloat() =>  _reader.ReadSingle();
        public double ReadDouble() =>  _reader.ReadDouble();
    }
}