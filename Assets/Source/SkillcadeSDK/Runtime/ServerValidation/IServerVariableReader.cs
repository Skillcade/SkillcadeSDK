#if UNITY_SERVER
using JetBrains.Annotations;

namespace SkillcadeSDK.ServerValidation
{
    [UsedImplicitly]
    public interface IServerVariableReader
    {
        public object Read(string value);
    }
}
#endif