using COTL_API.CustomStructures;
using CotLMiniMods.Interactions;
using CotLMiniMods.Structures;
using CotLTemplateMod;
using HarmonyLib;
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

            if (!CustomStructureManager.CustomStructures.ContainsKey(__instance.Type)) return;

            //temporary until someone adds it to the api
            if (CustomStructureManager.CustomStructures[__instance.Type] is HRManagementStructure)
            {
                Plugin.Log.LogInfo("adding interaction for candle debug");
                var parent = __instance.GetComponentInParent<Transform>();
                parent.gameObject.AddComponent<Interaction_HR>();
            }

            Plugin.Log.LogInfo("type is " + CustomStructureManager.CustomStructures[__instance.Type].GetType());


            
        }
    }
}
