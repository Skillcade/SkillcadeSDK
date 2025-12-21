using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

namespace SkillcadeSDK.Replays
{
    [Preserve]
    public static class ReplayDataObjectsRegistry
    {
        public static IReadOnlyDictionary<Type, int> TypeToId => _typeToId;
        public static IReadOnlyDictionary<int, Type> IdToType => _idToType;

        private static Dictionary<Type, int> _typeToId = new Dictionary<Type, int>();
        private static Dictionary<int, Type> _idToType = new Dictionary<int, Type>();
        
        [RuntimeInitializeOnLoadMethod]
        public static void CollectDataObjectTypes()
        {
            int id = 0;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var componentType in assembly.GetTypes().Where(t => !t.IsInterface && !t.IsAbstract && t.GetInterfaces().Contains(typeof(IReplayDataObject))))
                {
                    _idToType.Add(id, componentType);
                    _typeToId.Add(componentType, id);
                    id++;
                }
            }
        }
    }
}