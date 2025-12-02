using System;

namespace SkillcadeSDK.ServerValidation
{
#if UNITY_SERVER
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ServerPayloadVariableAttribute : Attribute
    {
        public string Name { get; set; }

        public ServerPayloadVariableAttribute(string name)
        {
            Name = name;
        }
    }
#endif
}