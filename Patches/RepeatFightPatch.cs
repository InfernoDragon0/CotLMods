using HarmonyLib;
using Lamb.UI;
using MMTools;
using System;
using System.Collections.Generic;
using System.Text;

namespace CotLTemplateMod.Patches
{
    [HarmonyPatch]
    internal class RepeatFightPatch
    {


        // This patch resets the chef state to fightable
        [HarmonyPatch(typeof(ChefShop), nameof(ChefShop.Start))]
        [HarmonyPrefix]
        public static void ChefShop_OnStart(UIPlayerUpgradesMenuController __instance)
        {
            if (!Plugin.shrimpChefRepeat.Value) return;
            
            if (DataManager.Instance.ShopKeeperChefState == 2)
            {
                DataManager.Instance.ShopKeeperChefState = 0;
            }
        }

        // This patch resets the death cat state to fightable
        [HarmonyPatch(typeof(DataManager), nameof(DataManager.SetNewRun))]
        [HarmonyPostfix]
        public static void DataManager_SetNewRun(UIPlayerUpgradesMenuController __instance)
        {
            if (!Plugin.deathCatRepeat.Value) return;

            DataManager.Instance.DeathCatBeaten = false;
        }
    }
}
