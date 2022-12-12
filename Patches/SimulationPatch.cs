using COTL_API.CustomStructures;
using CotLMiniMods.Interactions;
using CotLMiniMods.Structures;
using CotLMiniMods.Structures.Mines;
using CotLMiniMods.Structures.Productivity;
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
    internal class SimulationPatch
    {

        // This patch to add interactions to custom structures
        [HarmonyPatch(typeof(SimulationManager), nameof(SimulationManager.UnPause))]
        [HarmonyPrefix]
        public static bool SimulationManager_UnPause(SimulationManager __instance)
        {

            foreach (StructureBrain structureBrain in StructureManager.StructuresAtLocation(FollowerLocation.Base))
            {
                if (structureBrain is Structures_StasisChamber stasisChamber && stasisChamber.timeFrozen)
                {
                    Plugin.Log.LogInfo("Simulation is forced to stop by a stasis chamber.");
                    SimulationManager._isPaused = true;
                    return false;
                }
            }
            return true;
        }

    }
}
