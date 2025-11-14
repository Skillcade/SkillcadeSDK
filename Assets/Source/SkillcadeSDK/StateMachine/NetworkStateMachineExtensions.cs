using System;
using Unity.Collections.LowLevel.Unsafe;

namespace SkillcadeSDK.StateMachine
{
    public static class NetworkStateMachineExtensions
    {
        public static T GetType<T>(this StateData stateData) where T : Enum
        {
            return UnsafeUtility.As<int, T>(ref stateData.Type);
        }
        
        public static StateData WithType<T>(this StateData stateData, T value) where T : Enum
        {
            stateData.Type = UnsafeUtility.As<T, int>(ref value);
            return stateData;
        }
        
        public static StateData WithData(this StateData stateData, string value)
        {
            stateData.JsonData = value;
            return stateData;
        }
    }
}