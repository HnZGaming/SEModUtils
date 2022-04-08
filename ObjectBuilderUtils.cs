using System.Collections.Generic;
using Sandbox.Definitions;
using VRage.Game;
using VRage.ObjectBuilders;

namespace HNZ.Utils
{
    public static class ObjectBuilderUtils
    {
        static readonly Dictionary<string, Dictionary<string, MyPhysicalItemDefinition>> _physicalItemDefinitions;

        static ObjectBuilderUtils()
        {
            _physicalItemDefinitions = new Dictionary<string, Dictionary<string, MyPhysicalItemDefinition>>();
            foreach (var itemDefinition in MyDefinitionManager.Static.GetPhysicalItemDefinitions())
            {
                var typeId = itemDefinition.Id.TypeId.ToString().Split('_')[1];
                var subtypeId = itemDefinition.Id.SubtypeName;

                Dictionary<string, MyPhysicalItemDefinition> set;
                if (!_physicalItemDefinitions.TryGetValue(typeId, out set))
                {
                    set = _physicalItemDefinitions[typeId] = new Dictionary<string, MyPhysicalItemDefinition>();
                }

                set[subtypeId] = itemDefinition;
            }
        }

        public static bool TryCreatePhysicalObjectBuilder<T>(string typeId, string subtypeId, out T builder) where T : MyObjectBuilder_PhysicalObject
        {
            Dictionary<string, MyPhysicalItemDefinition> set;
            MyPhysicalItemDefinition def;
            if (_physicalItemDefinitions.TryGetValue(typeId, out set) && set.TryGetValue(subtypeId, out def))
            {
                builder = MyObjectBuilderSerializer.CreateNewObject(def.Id) as T;
                return builder != null;
            }

            builder = default(T);
            return false;
        }
    }
}