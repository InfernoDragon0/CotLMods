

using COTL_API.CustomTarotCard;
using CotLMiniMods;
using HarmonyLib;
using Lamb.UI;
using Sirenix.Serialization.Utilities;
using UnityEngine.SceneManagement;

// 1Augment of Death: When an enemy attacks, they have a 25% chance of spawning a poison puddle under them.
// 2Augment of Swarm: Double base enemy spawn (does not include augment spawns)
// 3Augment of Explosion: Enemies explode on death
// 4Augment of Bloodpact: When an enemy dies, other enemies gain 50% damage.
// 5Augment of Persistence: Enemies heal 10% of their health per 3 second. Bosses heal 2% of their health per second.
// 6Augment of Resistance: Enemies have a 25% chance of not taking damage from hits
// 7Augment of Dissonance: Enemy spawns are randomized
// 8Augment of Healing: 2 healers spawn per combat room
// 9Augment of Casuals: 1 juggernaut spawn per combat room
// 10Augment of Cultists: 5 random cultists spawn per combat room

// 11Trial of Narinder: Each active Augment will increase the health of all enemies by 50%, Each active Trial will grant enemies a 10% chance of instantly killing you on hit, and increases the health and damage of the final boss by 5% for each enemy you have killed during the run, and whenever a non boss enemy dies, they have a 5% chance of reviving if they are not the last enemy.
// 12Trial of Leshy: For each active Augment, whenever an enemy takes non lethal damage, there is a 5% chance an Amdusia kin will spawn. For each active Trial, additional Barbatos kins will appear in combat room.
// 13Trial of Heket: For each active Trial, Boss room will have an extra Mortar Hopper, and whenever you take damage, you have a 5% chance of losing a tarot card. For each active Augment, each time you take damage, you lose 1 max HP.
// 14Trial of Shamura: For each active Trial, each time you take damage, all enemies heals 10% health. For each active Augment, when enemies take damage, their damage increases by 5%.
// 15Trial of Kallamar: For each active Trial, this effect speeds up by 1 second. Every 6 seconds, all enemies drop a pool of poison. For each active Augment, this effect speeds up by 1 second. Every 11 seconds, a pool of poison is spawned on your location.

//patch enemy.Attack TrapPoison.CreatePoison(base.transform.position, 1, 0f, GenerateRoom.Instance.transform, false);
//patch Enemy.OnDie 3 explode, 4 give all other enemies +50% dmg

namespace CotLTemplateMod.Patches
{
    [HarmonyPatch]

    internal class TrialsOfTheGodPatch
    {
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
