using BepInEx.Logging;
using HarmonyLib;
using Lamb.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace CotLTemplateMod.Patches
{
    [HarmonyPatch]
    internal class ShrinePatch
    {
        // This patch changes all shrine max to 2000
        [HarmonyPatch(typeof(Structures_Shrine), "SoulMax", MethodType.Getter)]
        [HarmonyPostfix]
        public static void SoulMax_Override(ref int __result)
        {
            if (!Plugin.biggerShrine.Value) return;
            __result = 2000;
        }

        // This patch doubles each shrine's max prayer spots 
        /*[HarmonyPatch(typeof(Structures_Shrine), "PrayerMax", MethodType.Getter)]
        [HarmonyPostfix]
        public static void PrayerMax_Override(ref int __result)
        {
            __result *= 2;
        }*/


        // This patch increases the amount of souls provided by the follower
        [HarmonyPatch(typeof(FollowerTask_Pray), nameof(FollowerTask_Pray.DepositSoul))]
        [HarmonyPrefix]
        public static void DepositSoul_More(FollowerTask_Pray __instance)
        {
            if (!Plugin.biggerShrine.Value) return;
            __instance._shrine.SoulCount += 1;
        }

        // This patch instantly grants all the shrine souls to you
        [HarmonyPatch(typeof(BuildingShrine), nameof(BuildingShrine.GivePlayerSoul))]
        [HarmonyPostfix]
        public static void GivePlayerSoul_CollectAll(BuildingShrine __instance)
        {
            if (!Plugin.oneClickShrineCollection.Value) return;
            Plugin.Log.LogInfo("collecting souls " + __instance.StructureBrain.SoulCount);
            for (int i = 0; i < __instance.StructureBrain.SoulCount; i++)
            {
                PlayerFarming.Instance?.GetSoul(1);
            }
            
            __instance.StructureBrain.SoulCount = 0;
            __instance.UpdateBar();
        }

    }
}
