using COTL_API.CustomTarotCard;
using CotLMiniMods;
using HarmonyLib;
using I2.Loc;
using Lamb.UI;
using MMRoomGeneration;
using MMTools;
using Sirenix.Serialization.Utilities;
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
                    Plugin.Log.LogInfo("Equipping relic to multiplayer");
                    PlayerFarming.players[0].currentRelicType = Plugin.relicData;
                    PlayerFarming.players[1].currentRelicType = Plugin.relicData;

                    try
                    {
                        PlayerFarming.players[0].playerRelic.EquipRelic(relicData, false, true);
                        PlayerFarming.players[1].playerRelic.EquipRelic(relicData, false, true);
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.LogError("Error equipping relic to players: " + ex);
                    }

                    EquipRelic(relicData, PlayerFarming.players[0]);
                    EquipRelic(relicData, PlayerFarming.players[1]);

                }
                else
                {
                    Plugin.Log.LogInfo("Equipping relic to singleplayer");
                    __instance.currentRelicType = Plugin.relicData;
                    try
                    {
                        __instance.playerRelic.EquipRelic(relicData, false, true);
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.LogError("Error equipping relic to players: " + ex);
                    }

                    EquipRelic(relicData, __instance);
                    Plugin.Log.LogInfo("after equip relic");
                }

                Plugin.relicData = RelicType.None;
            }
        }

        public static void EquipRelic(RelicData relicData, PlayerFarming player)
        {
            Plugin.Log.LogInfo("Manual equipping relic to player " + player);
            var playerRelic = player.playerRelic;
            playerRelic.CurrentRelic = relicData;
            // playerRelic.ChargedAmount = Mathf.Clamp(float.MaxValue, 0.0f, playerRelic.RequiredChargeAmount);
            switch (relicData.RelicType)
                {
                    case RelicType.UseRandomRelic:
                        EquipmentManager.PickRandomRelicData(true);
                        Plugin.Log.LogInfo("Picked random relic");
                        break;
                    case RelicType.UseRandomRelic_Blessed:
                        EquipmentManager.PickRandomRelicData(true, RelicSubType.Blessed);
                        Plugin.Log.LogInfo("Picked random blessed relic");
                        break;
                    case RelicType.UseRandomRelic_Dammed:
                        EquipmentManager.PickRandomRelicData(true, RelicSubType.Dammed);
                        Plugin.Log.LogInfo("Picked random dammed relic");
                        break;
                }

        }

        [HarmonyPatch(typeof(UITarotChoiceOverlayController), nameof(UITarotChoiceOverlayController.Show))]
        [HarmonyBefore(["InfernoDragon0.cotl.SuperchargedTarots"])]
        [HarmonyPrefix]
        public static bool UITarotChoiceOverlayController_Show(UITarotChoiceOverlayController __instance,
                TarotCards.TarotCard card1, TarotCards.TarotCard card2, bool instant)
        {
            Plugin.Log.LogInfo("Hiding proxy cards");
            Plugin.proxyTrials.Keys.ForEach(x => CustomTarotCardManager.CustomTarotCardList.Remove(x));
            Plugin.proxyAugments.Keys.ForEach(x => CustomTarotCardManager.CustomTarotCardList.Remove(x));
            return true;
        }

        [HarmonyPatch(typeof(UITarotChoiceOverlayController), nameof(UITarotChoiceOverlayController.Show))]
        [HarmonyPostfix]
        public static void UITarotChoiceOverlayController_Show_Post(UITarotChoiceOverlayController __instance,
                TarotCards.TarotCard card1, TarotCards.TarotCard card2, bool instant)
        {
            Plugin.Log.LogInfo("Restoring proxy cards");
            Plugin.proxyTrials.ForEach(x => CustomTarotCardManager.CustomTarotCardList.Add(x.Key, x.Value));
            Plugin.proxyAugments.ForEach(x => CustomTarotCardManager.CustomTarotCardList.Add(x.Key, x.Value));
        }
    }
}
