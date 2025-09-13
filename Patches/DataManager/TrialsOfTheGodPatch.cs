

using COTL_API.CustomTarotCard;
using CotLMiniMods;
using HarmonyLib;
using Lamb.UI;
using MMRoomGeneration;
using Sirenix.Serialization.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

// 1Augment of Death (DONE): When an enemy attacks, they have a 25% chance of spawning a poison puddle under them.
// 2Augment of Swarm (DONE): Enemies move 100% faster
// 3Augment of Explosion: Enemies explode on death
// 4Augment of Bloodpact: When an enemy dies, other enemies gain 50% damage.
// 5Augment of Persistence: Enemies heal 5% of their health per 3 second. Bosses heal 1% of their health per second.
// 6Augment of Resistance: Enemies have a 25% chance of not taking damage from hits
// 7Augment of Dissonance: Dodging costs 20% curse charge, you cannot dodge if you have less than 20% curse charge.
// 8Augment of Curse: Each time you attack, you lose 5% curse charge
// 9Augment of Grace: When an enemy dies, all other enemies double their current and max hp.
// 10Augment of Bombardment: Each time you attack, 2 enemy bombs appear around you.

// 11Trial of Narinder: Each active Augment will increase the health of all enemies by 50%, Each active Trial will grant enemies a 10% chance of instantly killing you on hit, and increases the health and damage of the final boss by 5% for each enemy you have killed during the run, and whenever a non boss enemy dies, they have a 5% chance of reviving if they are not the last enemy.
// 12Trial of Leshy: For each active Augment, whenever an enemy takes non lethal damage, there is a 5% chance for them to duplicate. For each active Trial, a copy of each enemy will spawn.
// 13Trial of Heket (BOTH DONE): For each active Trial, whenever you take damage, you have a 10% chance of losing a tarot card, if you have no tarot cards, you lose 1 max HP. For each active Augment, each time you take damage, you lose 10% of curse charge.
// 14Trial of Shamura: For each active Trial, each time you take damage, all enemies heals 10% health. For each active Augment, when enemies take damage, their damage increases by 5%.
// 15Trial of Kallamar: For each active Trial, this effect speeds up by 1 second. Every 6 seconds, all enemies drop a pool of poison. For each active Augment, this effect speeds up by 1 second. Every 11 seconds, a pool of poison is spawned on your location.

//(1) patch HealthPlayer.DealDamage TrapPoison.CreatePoison(base.transform.position, 1, 0f, GenerateRoom.Instance.transform, false);
//(2) patch UnitObject.maxSpeed *= 2; UnitObject.SpeedMultiplier *= 2;
//(3) patch Enemy.OnDie explode
//(4) patch Enemy.OnDie give all other enemies +50% dmg
//(5) patch Enemy.Update heal 5% per 3s, if boss then 1% per second (UnitObject.isBoss)
//(6) patch Health.DealDamage if Enemy team, 25% chance to ignore damage
//(7) patch Playerfarming.DodgeRoll PRE: check if curse charge is at least 20%, cannot roll without curse charge. POST: if result is true, take 20% curse charge
//(8) patch PlayerWeapon.DoAttackRoutine take 5% curse charge
//(9) patch Enemy.OnDie double hp of all other enemies
//(10) patch PlayerWeapon.DoAttackRoutine spawn 2 bombs around player

//(11) patch 1: Enemy.Start add 50% max hp and heal to full per active augment, patch 2: Health.DealDamage if enemy hits player, 10% chance to insta kill per active trial, patch 3: Enemy.Start if UnitObject.isBoss, boss gains 5% hp and damage per enemy kill, patch 4: Enemy.OnDie if not last enemy, 5% chance to respawn. Enemy.OnDie track enemy kills
//(12) patch 1: Health.DealDamage if non lethal damage to enemy, 5% chance to duplicate at same health before taking damage per active augment, patch 2: Enemy.Start instantiate copy of each enemy per active trial
//(13) patch 1: HealthPlayer.DealDamage if player is hit, for each active trial, 10% chance to lose tarot card, if no tarot card, lose 1 max hp, patch 2: Health.DealDamage if player is hit, for each active augment, lose 10% curse charge
//(14) patch 1: HealthPlayer.DealDamage if player is hit, for each active trial, all enemies heal 10% hp, patch 2: Health.DealDamage if enemy is hit, for each active augment, enemy gains 5% damage
//(15) patch 1: every 6 - 1 per trial seconds, TrapPoison.CreatePoison, patch 2: every 11 - 1 per augment seconds, TrapPoison.CreatePoison at player per active augment

//BiomeGenerator.ChangeRoomRoutine
//BiomeRoom.Activate gets called after that, activating the next room and setactive false to the previous room
//GenerateRoom.Generate is called by Activate, it will run coroutine Generate
//Generate will spawn the room content and the mobs
//GenerateRoom.GenerateEvent gets called after Generate, maybe inject here??
//get the current room type. if it is a combat room, then spawn extra mobs
//The room is empty until BiomeRoom.Activate via entering the room collider
//All the rooms are actually physically in the same spot
//disableisland will generate the map and enemies. the enemies are part of the island prefab. they do not spawn via spawner

//AssetReferenceGameObject
// UnitObject component = ObjectPool.Spawn(result, enemyFrogBoss.transform.parent.parent, enemyFrogBoss.miniBossSpawnPositions[index], Quaternion.identity).GetComponent<UnitObject>();
// component.gameObject.SetActive(false);
//EnemySpawner.CreateWithAndInitInstantiatedEnemy(component.transform.position, enemyFrogBoss.transform.parent, component.gameObject); spawns an enemy
//we need to find component.gameObject to spawn, by prefabs


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

        //HealthPlayer.DealDamage: the player takes damage. patches for trials 13 and 14, and augment 1
        [HarmonyPatch(typeof(HealthPlayer), nameof(HealthPlayer.DealDamage))]
        [HarmonyPostfix]
        public static void HealthPlayer_DealDamage(ref bool __result, HealthPlayer __instance, GameObject Attacker, Vector3 AttackLocation)
        {
            if (__result)
            {
                //augment 1: when an enemy attacks, they have a 25% chance of spawning a poison puddle under them.
                if (Plugin.proxyAugmentsEnabled.Contains(Plugin.DeathCard))
                {
                    if (Random.Range(0, 100) < 25)
                    {
                        Plugin.Log.LogInfo("Augment of Death triggered");
                        TrapPoison.CreatePoison(__instance.transform.position, 1, 1.5f, GenerateRoom.Instance.transform, false);
                    }
                }
                //trial 13: for each active trial, 10% chance to lose tarot card, if no tarot card, lose 1 max hp. for each active augment, lose 10% curse charge
                if (Plugin.proxyTrialsEnabled.Contains(Plugin.HeketCard))
                {
                    //trial of Heket, trial trigger
                    var chance = 10 * Plugin.proxyTrialsEnabled.Count;
                    if (Random.Range(0, 100) < chance)
                    {
                        Plugin.Log.LogInfo("Trial of Heket (Trial) triggered");
                        foreach (var player in PlayerFarming.players)
                        {
                            if (player.RunTrinkets.Count > 0)
                            {
                                Plugin.Log.LogInfo("Trial Trigger: Removing tarot card");
                                TrinketManager.RemoveRandomTrinket(player);
                            }
                            else
                            {
                                Plugin.Log.LogInfo("Trial Trigger: No tarot card to remove, losing 1 max HP");
                                player.health.totalHP -= 1;
                            }
                        }
                    }
                    //trial of Heket, augment trigger
                    var curseLoss = 10 * Plugin.proxyAugmentsEnabled.Count;
                    if (curseLoss > 0)
                    {
                        Plugin.Log.LogInfo("Trial of Heket (Augment) triggered, losing " + curseLoss + " curse charges");
                        foreach (var player in PlayerFarming.players)
                        {
                            player.playerSpells.faithAmmo.Ammo = Mathf.Clamp(player.playerSpells.faithAmmo.Ammo - (player.playerSpells.faithAmmo.Total * (curseLoss / 100f)), 0, player.playerSpells.faithAmmo.Total); ;
                        }
                    }
                }

                //trial 14: for each active trial, all enemies heal 10% hp
                if (Plugin.proxyTrialsEnabled.Contains(Plugin.ShamuraCard))
                {
                    var healPercent = 0.1f * Plugin.proxyTrialsEnabled.Count;
                    Plugin.Log.LogInfo("Trial of Shamura (Trial) triggered, healing all enemies by " + (healPercent * 100) + "%");

                }

            }
        }

        //UnitObject.OnEnable: called when an enemy is enabled, for augment 2
        [HarmonyPatch(typeof(UnitObject), nameof(UnitObject.OnEnable))]
        [HarmonyPostfix]
        public static void UnitObject_OnEnable(UnitObject __instance)
        {
            if (__instance.health.team != Health.Team.Team2)
                return;

            if (Plugin.proxyAugmentsEnabled.Contains(Plugin.SwarmCard))
            {
                Plugin.Log.LogInfo("Augment of Swarm triggered on " + __instance.name);
                __instance.maxSpeed *= 5;
                __instance.SpeedMultiplier *= 5;
            }
        }
            
    }
}
