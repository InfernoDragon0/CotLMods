using COTL_API.CustomStructures;
using HarmonyLib;
using Lamb.UI.BuildMenu;
using Lamb.UI.FollowerInteractionWheel;
using System;
using System.Collections.Generic;
using System.Text;

namespace CotLTemplateMod.Patches
{
    [HarmonyPatch]
    internal class FishingStructurePatch
    {
        //also what is ALCHEMY_CAULDRON and INTERACTION_APOTHECARY?
        //TODO: HUT 2 is the exact same as Hut 1, so find something different to create

        //name
        [HarmonyPatch(typeof(StructuresData), "LocalizedName", new Type[] { typeof(StructureBrain.TYPES) })]
        [HarmonyPrefix]
        public static bool StructuresData_LocalizedName(StructureBrain.TYPES Type, ref string __result)
        {
            if (Type == StructureBrain.TYPES.FISHING_HUT)
            {
                Plugin.Log.LogInfo("found fishing hut 1");
                __result = "Fishing Hut I";
                return false;
            }
            else if (Type == StructureBrain.TYPES.FISHING_HUT_2)
            {
                Plugin.Log.LogInfo("found fishing hut 2");
                __result = "Fishing Hut II";
                return false;
            }
            else
            {
                return true;
            }
        }

        //description
        [HarmonyPatch(typeof(StructuresData), "LocalizedDescription", new Type[] { typeof(StructureBrain.TYPES) })]
        [HarmonyPrefix]
        public static bool StructuresData_LocalizedDescription(StructureBrain.TYPES Type, ref string __result)
        {
            if (Type == StructureBrain.TYPES.FISHING_HUT)
            {
                __result = "Create a hut that allows you to fish for a wide range of seafood related ingredients!";
                return false;
            }
            else if (Type == StructureBrain.TYPES.FISHING_HUT_2)
            {
                __result = "A bigger place for followers to fish";
                return false;
            }
            else
            {
                return true;
            }
        }

        //enable fishing task
        [HarmonyPatch(typeof(FollowerBrain), "IsTaskAvailable")]
        [HarmonyPostfix]
        private static void FollowerBrain_IsTaskAvailable(ref bool __result, FollowerTaskType taskType)
        {
            if (taskType == FollowerTaskType.Fisherman)
            {
                __result = true;
            }

        }
    }
}
