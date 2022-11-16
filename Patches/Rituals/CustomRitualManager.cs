using COTL_API.CustomStructures;
using COTL_API.Guid;
using CotLMiniMods.Rituals;
using CotLTemplateMod;
using System.Collections.Generic;
using System.Reflection;

namespace CotLMiniMods.Patches.Rituals
{
    internal class CustomRitualManager
    {
        public static Dictionary<UpgradeSystem.Type, CustomRitual> CustomRituals { get; } = new();

        public static UpgradeSystem.Type Add(CustomRitual ritual)
        {
            string guid = TypeManager.GetModIdFromCallstack(Assembly.GetCallingAssembly());

            UpgradeSystem.Type upgradeType = GuidManager.GetEnumValue<UpgradeSystem.Type>(guid, ritual.InternalName);
            ritual.ModPrefix = guid;
            ritual.upgradeType = upgradeType;

            CustomRituals.Add(upgradeType, ritual);
            return upgradeType;
        }
    }
}
