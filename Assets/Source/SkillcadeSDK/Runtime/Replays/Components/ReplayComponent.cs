using UnityEngine;

namespace SkillcadeSDK.Replays.Components
{
    public abstract class ReplayComponent : MonoBehaviour
    {
        public abstract int Id { get; }
        public abstract int Size { get; }

        public abstract void Read(ReplayReader reader);
        public abstract void Write(ReplayWriter writer);
    }
}