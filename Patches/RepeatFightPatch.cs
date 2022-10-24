using HarmonyLib;
using I2.Loc;
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

        //this patch allows more than 1 the one who waits in the base
        [HarmonyPatch(typeof(Interaction_FollowerSpawn), nameof(Interaction_FollowerSpawn.Play))]
        [HarmonyPostfix]
        public static void Interaction_FollowerSpawn_Play(Interaction_FollowerSpawn __instance, string ForceName)
        { //hopefully a non invasive edit will work
            if (!Plugin.deathCatRepeat.Value) return;

            if (FollowerBrain.FindBrainByID(666) == null)
            {
                Plugin.Log.LogInfo("This is the original copy of death cat");
                //return; //if we want a vanilla death cat follower on the first kill, we should return
            }

            if (ForceName == ScriptLocalization.NAMES.DeathNPC)
            {
                __instance._followerInfo.ID = ++DataManager.Instance.FollowerID;

                //rare event that you have 666 followers
                if (__instance._followerInfo.ID == 666)
                {
                    __instance._followerInfo.ID = ++DataManager.Instance.FollowerID;
                }

                __instance._followerInfo.Name = "Impostor Death Cat";
                FollowerBrain.GetOrCreateBrain(__instance._followerInfo).AddTrait(FollowerTrait.TraitType.Immortal);

                System.Action followerInfoAssigned = __instance.followerInfoAssigned;
                if (followerInfoAssigned == null)
                    return;
                followerInfoAssigned();
            }

        }

        //this patch to fix death cat followers who are already in the recruit zone
        [HarmonyPatch(typeof(FollowerManager), nameof(FollowerManager.SpawnRecruit))]
        [HarmonyPrefix]
        public static void FollowerManager_SpawnRecruit(FollowerManager __instance, FollowerInfo f)
        {
            if (!Plugin.deathCatRepeat.Value) return;

            if (FollowerBrain.FindBrainByID(666) == null)
            {
                Plugin.Log.LogInfo("This is the original copy of death cat");
                //return; //if we want a vanilla death cat follower on the first kill, we should return
            }
            
            if (f.ID == 666)
            {
                while (f.ID == 666)
                {
                    f.ID = ++DataManager.Instance.FollowerID;
                    Plugin.Log.LogInfo("set the new follower id to " + f.ID);
                }
                FollowerBrain.GetOrCreateBrain(f).AddTrait(FollowerTrait.TraitType.Immortal);
            }

        }

    }
}
