using COTL_API.CustomStructures;
using CotLMiniMods.Interactions;
using CotLMiniMods.Structures;
using CotLMiniMods.Structures.Mines;
using CotLTemplateMod;
using HarmonyLib;
using src.UI.Menus;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CotLMiniMods.Patches
{
    [HarmonyPatch]
    internal class StructurePatch
    {

        // This patch to add interactions to custom structures
        [HarmonyPatch(typeof(Structure), nameof(Structure.Start))]
        [HarmonyPostfix]
        public static void Structure_Start(Structure __instance)
        {

            if (!CustomStructureManager.CustomStructureList.ContainsKey(__instance.Type)) return;

            //TODO: make a custom_structure_with_interaction or something and add component through that instead
            if (CustomStructureManager.CustomStructureList[__instance.Type] is HRManagementStructure)
            {
                Plugin.Log.LogInfo("adding interaction for HR");
                var parent = __instance.GetComponentInParent<Transform>();
                parent.gameObject.AddComponent<Interaction_HR>();
            }
            else if (CustomStructureManager.CustomStructureList[__instance.Type] is CrystalMineStructure)
            {
                Plugin.Log.LogInfo("adding interaction for crystal");
                var parent = __instance.GetComponentInParent<Transform>();
                parent.gameObject.AddComponent<Interaction_CrystalMine>();
            }
            else if (CustomStructureManager.CustomStructureList[__instance.Type] is BoneMineStructure)
            {
                Plugin.Log.LogInfo("adding interaction for bone");
                var parent = __instance.GetComponentInParent<Transform>();
                parent.gameObject.AddComponent<Interaction_CrystalMine>();
            }
            else if (CustomStructureManager.CustomStructureList[__instance.Type] is SilkMineStructure)
            {
                Plugin.Log.LogInfo("adding interaction for web");
                var parent = __instance.GetComponentInParent<Transform>();
                parent.gameObject.AddComponent<Interaction_CrystalMine>();
            }
            else if (CustomStructureManager.CustomStructureList[__instance.Type] is WishingWellStructure)
            {
                Plugin.Log.LogInfo("adding interaction for wish");
                var parent = __instance.GetComponentInParent<Transform>();
                parent.gameObject.AddComponent<Interaction_Wish>();
            }
            else if (CustomStructureManager.CustomStructureList[__instance.Type] is Structures_AlchemyCauldron)
            {
                Plugin.Log.LogInfo("adding interaction for alchemy cauldron");
                var parent = __instance.GetComponentInParent<Transform>();
                parent.gameObject.AddComponent<Interaction_AlchemyCauldron>();
            }

            Plugin.Log.LogInfo("type is " + CustomStructureManager.CustomStructureList[__instance.Type].GetType());


            
        }

    }
}
