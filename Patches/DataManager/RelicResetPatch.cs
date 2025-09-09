using COTL_API.CustomTarotCard;
using CotLMiniMods;
using HarmonyLib;
using I2.Loc;
using Lamb.UI;
using MMRoomGeneration;
using MMTools;
using Socket.Newtonsoft.Json.Utilities.LinqBridge;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CotLTemplateMod.Patches
{
    [HarmonyPatch]

    internal class RelicResetPatch
    {
        public static RelicType temp = RelicType.None;

        // This patch edits bell interaction
        // [HarmonyPatch(typeof(DataManager), nameof(DataManager.SetNewRun))]
        // [HarmonyPrefix]
        // public static void DataManager_SetNewRun(out RelicType __state)
        // {
        //     //collect the loot
        //     if (Plugin.relicNoReset.Value)
        //     {
        //         Plugin.Log.LogInfo("GOT current relic (SINGLEPLAYER ONLY)");
        //         Plugin.Log.LogInfo(PlayerFarming.Instance.currentRelicType);
        //         __state = PlayerFarming.Instance.currentRelicType;
        //         temp = PlayerFarming.Instance.currentRelicType;
        //     }
        //     else
        //     {
        //         __state = RelicType.None;
        //     }
        // }

        // [HarmonyPatch(typeof(DataManager), nameof(DataManager.SetNewRun))]
        // [HarmonyPostfix]
        // public static void DataManager_SetNewRun_Post(RelicType __state)
        // {
        //     //collect the loot
        //     if (__state != RelicType.None)
        //     {
        //         Plugin.Log.LogInfo("GOT old relic");
        //         Plugin.Log.LogInfo(__state);
        //         PlayerFarming.Instance.currentRelicType = __state;
        //         PlayerFarming.Instance.playerRelic.EquipRelic(EquipmentManager.GetRelicData(PlayerFarming.Instance.currentRelicType), false);
        //     }
        // }

        // [HarmonyPatch(typeof(PlayerFarming), nameof(PlayerFarming.Awake))]
        // [HarmonyPostfix]
        // public static void DataManager_ResetRunData_Post()
        // {
        //     //collect the loot
        //     Plugin.Log.LogInfo("Forced Relic");
        //     Plugin.Log.LogInfo(temp);

        //     if (temp != RelicType.None)
        //     {
        //         PlayerFarming.Instance.playerRelic.EquipRelic(EquipmentManager.GetRelicData(temp), false);
        //     }
        // }

        //Tarot Reapply when playerfarming starts again in game scene
        [HarmonyPatch(typeof(PlayerFarming), nameof(PlayerFarming.Start))]
        [HarmonyPostfix]
        public static void PlayerFarming_Start_(PlayerFarming __instance)
        {
            Plugin.Log.LogInfo("Scene currently is " + SceneManager.GetActiveScene().name);
            if (!SceneManager.GetActiveScene().name.Contains("Dungeon")) //todo find the active scene for game
                return;
            //collect the loot
            Plugin.Log.LogInfo("Reapplying wished cards, count is " + Plugin.wishedCards.Count);
            foreach (var card in Plugin.wishedCards)
            {
                Plugin.Log.LogInfo("Reapplying wished card " + card.CardType);
                if (CoopManager.CoopActive)
                {
                    //wishing well will add to both players
                    TrinketManager.AddTrinket(card, PlayerFarming.players[0]);
                    TrinketManager.AddTrinket(card, PlayerFarming.players[1]);
                }
                else
                {
                    TrinketManager.AddTrinket(card, __instance);
                }
            }
            Plugin.wishedCards.Clear();

            if (Plugin.relicData != RelicType.None)
            {
                Plugin.Log.LogInfo("Adding wished relic " + Plugin.relicData);
                //create relic
                var relicData = EquipmentManager.GetRelicData(Plugin.relicData);
                if (relicData == null)
                {
                    Plugin.Log.LogInfo("RelicData is null, skipping relic patch");
                    return;
                }

                if (CoopManager.CoopActive)
                {
                    //wishing well will add to both players
                    PlayerFarming.players[0].currentRelicType = Plugin.relicData;
                    PlayerFarming.players[0].playerRelic.EquipRelic(relicData, false);
                    PlayerFarming.players[1].currentRelicType = Plugin.relicData;
                    PlayerFarming.players[1].playerRelic.EquipRelic(relicData, false);
                }
                else
                {
                    __instance.currentRelicType = Plugin.relicData;
                    __instance.playerRelic.EquipRelic(relicData, true);
                }

                Plugin.relicData = RelicType.None;
            }
        }

        [HarmonyPatch(typeof(UITarotChoiceOverlayController), nameof(UITarotChoiceOverlayController.Show))]
        [HarmonyBefore(["InfernoDragon0.cotl.SuperchargedTarots"])]
        [HarmonyPrefix]
        public static bool UITarotChoiceOverlayController_Show(UITarotChoiceOverlayController __instance,
                TarotCards.TarotCard card1, TarotCards.TarotCard card2, bool instant)
        {
            Plugin.proxyAugmentsEnabled.ForEach(aug => CustomTarotCardManager.CustomTarotCardList.Remove(aug));
            Plugin.proxyTrialsEnabled.ForEach(trial => CustomTarotCardManager.CustomTarotCardList.Remove(trial));
            return true;
        }

        [HarmonyPatch(typeof(UITarotChoiceOverlayController), nameof(UITarotChoiceOverlayController.Show))]
        [HarmonyPostfix]
        public static bool UITarotChoiceOverlayController_Show_Post(UITarotChoiceOverlayController __instance,
                TarotCards.TarotCard card1, TarotCards.TarotCard card2, bool instant)
        {
            Plugin.proxyAugmentsEnabled.ForEach(aug => CustomTarotCardManager.CustomTarotCardList.Remove(aug));
            Plugin.proxyTrialsEnabled.ForEach(trial => CustomTarotCardManager.CustomTarotCardList.Remove(trial));
            return true;
        }
    }
}
