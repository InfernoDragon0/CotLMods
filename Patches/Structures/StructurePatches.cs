using COTL_API.CustomStructures;
using CotLTemplateMod;
using HarmonyLib;
using Lamb.UI.BuildMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CotLMiniMods.Patches.Structures
{
    [HarmonyPatch]
    public class StructurePatches
    {
        [HarmonyPatch(typeof(StructureBrain), "CreateBrain")]
        [HarmonyPrefix]
        private static bool StructureBrain_CreateBrain(ref StructureBrain __result, StructuresData data)
        {
            if (!CustomStructureManager.CustomStructures.ContainsKey(data.Type)) return true;
            Plugin.Log.LogInfo("CAN OVVERIDE BRAIN");
            //StructureBrain structureBrain = new StructureBrain();
            Type t = CustomStructureManager.CustomStructures[data.Type].GetType();
            StructureBrain structureBrain = Activator.CreateInstance(t) as StructureBrain;

            StructureBrain.ApplyConfigToData(data);
            structureBrain.Init(data);
            StructureBrain._brainsByID[data.ID] = structureBrain;
            StructureManager.StructuresAtLocation(data.Location).Add(structureBrain);
            __result = structureBrain;
            return false;
        }
    }
}
