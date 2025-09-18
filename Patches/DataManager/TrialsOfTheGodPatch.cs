

using System.Collections;
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
// 3Augment of Explosion (DONE): Enemies explode on death
// 4Augment of Bloodpact (DONE): When an enemy dies, other enemies gain 10% damage.
// 5Augment of Persistence (DONE): Enemies heal 5% of their health per 3 second. Bosses heal 1% of their health per second.
// 6Augment of Resistance (DONE): Enemies have a 25% chance of not taking damage from hits
// 7Augment of Dissonance (DONE): Dodging costs 20% curse charge, you cannot dodge if you have less than 20% curse charge.
// 8Augment of Curse (DONE): Each time you attack, you lose 5% curse charge
// 9Augment of Grace (DONE): When an enemy dies, all other enemies max hp +40%, heal 25%.
// 10Augment of Bombardment (DONE): Each time you attack, 2 bombs appear around you.

// 11Trial of Narinder (ALL DONE): Each active Augment will increase the health of all enemies by 50%, Each active Trial will grant enemies a 10% chance of instantly killing you on hit, and increases the health of the final boss by 5% for each enemy you have killed during the run.
// 12Trial of Leshy (BOTH DONE): For each active Augment, whenever an enemy takes non lethal damage, there is a 5% chance for them to duplicate. For each active Trial, a copy of each non-boss enemy will spawn every 10 seconds.
// 13Trial of Heket (BOTH DONE): For each active Trial, whenever you take damage, you have a 10% chance of losing a tarot card, if you have no tarot cards, you lose 1 max HP. For each active Augment, each time you take damage, you lose 10% of curse charge.

// 15Trial of Kallamar (BOTH DONE): For each active Trial, this effect speeds up by 1 second. Every 6 seconds, all enemies drop a pool of poison. For each active Augment, this effect speeds up by 1 second. Every 11 seconds, a pool of poison is spawned on your location.

// 14Trial of Shamura (BOTH DONE untested): For each active Trial, each time you take damage, all enemies heals 10% health. For each active Augment, when enemies take damage, their damage resistance increases by 0.5%.


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
        public static int killCount = 0;
        public static int timer = 0;

        public static int mobLimit = 20;
        //Tarot Reapply when playerfarming starts again in game scene
        //(15) patch 2: PlayerFarming.Start every 11 - 1 per augment seconds, TrapPoison.CreatePoison at player per active augment

        [HarmonyPatch(typeof(PlayerFarming), nameof(PlayerFarming.Start))]
        [HarmonyPostfix]
        public static void PlayerFarming_Start_(PlayerFarming __instance)
        {
            Plugin.Log.LogInfo("Scene currently is " + SceneManager.GetActiveScene().name);
            Plugin.Log.LogInfo("Reset kill count");
            killCount = 0;

            //log active trials and augments
            Plugin.Log.LogInfo("Active Trials: " + Plugin.proxyTrialsEnabled.Count);
            foreach (var trial in Plugin.proxyTrialsEnabled)
            {
                Plugin.Log.LogInfo("Active Trial: " + trial);
            }
            Plugin.Log.LogInfo("Active Augments: " + Plugin.proxyAugmentsEnabled.Count);
            foreach (var augment in Plugin.proxyAugmentsEnabled)
            {
                Plugin.Log.LogInfo("Active Augment: " + augment);
            }

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

            //(15) patch 2: PlayerFarming.Start every 11 - 1 per augment seconds, TrapPoison.CreatePoison at player per active augment
            //(15) patch 1: UnitObject.OnEnable every 6 - 1 per trial seconds, TrapPoison.CreatePoison
            if (Plugin.proxyTrialsEnabled.Contains(Plugin.KallamarCard))
            {
                Plugin.Log.LogInfo("Trial of Kallamar (augment) triggered");
                var timer = Mathf.Clamp(11 - Plugin.proxyAugmentsEnabled.Count, 1, 11);
                var timer2 = Mathf.Clamp(6 - Plugin.proxyTrialsEnabled.Count, 1, 6);
                __instance.StartCoroutine(PoisonTrialPlayer(timer, __instance.gameObject));
                __instance.StartCoroutine(PoisonTrial(timer2));

            }

            //(5) patch Enemy.Update heal 5% per 3s, if boss then 1% per second (UnitObject.isBoss)
            if (Plugin.proxyAugmentsEnabled.Contains(Plugin.PersistenceCard))
            {
                Plugin.Log.LogInfo("Augment of Persistence triggered on " + __instance.name);
                __instance.StartCoroutine(HealthRegen());
            }

            //(12) patch 2: GenerateRoom.Generate postfix instantiate copy of each enemy per active trial
            if (Plugin.proxyTrialsEnabled.Contains(Plugin.LeshyCard))
            {
                Plugin.Log.LogInfo("Trial of Leshy (Trial) triggered " + __instance.name);
                __instance.StartCoroutine(LeshySpawnDuplicateTrial());
            }

        }

        public static IEnumerator PoisonTrialPlayer(float duration, GameObject gameObject)
        {
            while (true)
            {
                yield return new WaitForSeconds(duration);
                TrapPoison.CreatePoison(gameObject.transform.position, 1, 1, gameObject.transform.parent);
            }
        }

        public static IEnumerator PoisonTrial(float duration)
        {
            while (true)
            {
                yield return new WaitForSeconds(duration);
                var enemies = GameObject.FindObjectsOfType<UnitObject>();
                foreach (var unit in enemies)
                {
                    if (unit.health.team == Health.Team.Team2)
                    {
                        TrapPoison.CreatePoison(unit.transform.position, 1, 1, unit.transform.parent);
                    }
                }
            }
        }

        // [HarmonyPatch(typeof(UITarotChoiceOverlayController), nameof(UITarotChoiceOverlayController.Show))]
        // [HarmonyBefore(["InfernoDragon0.cotl.SuperchargedTarots"])]
        // [HarmonyPrefix]
        // public static bool UITarotChoiceOverlayController_Show(UITarotChoiceOverlayController __instance,
        //         TarotCards.TarotCard card1, TarotCards.TarotCard card2, bool instant)
        // {
        //     Plugin.Log.LogInfo("Hiding proxy cards");
        //     Plugin.proxyTrials.Keys.ForEach(x => CustomTarotCardManager.CustomTarotCardList.Remove(x));
        //     Plugin.proxyAugments.Keys.ForEach(x => CustomTarotCardManager.CustomTarotCardList.Remove(x));
        //     return true;
        // }

        // [HarmonyPatch(typeof(UITarotChoiceOverlayController), nameof(UITarotChoiceOverlayController.Show))]
        // [HarmonyPostfix]
        // public static void UITarotChoiceOverlayController_Show_Post(UITarotChoiceOverlayController __instance,
        //         TarotCards.TarotCard card1, TarotCards.TarotCard card2, bool instant)
        // {
        //     Plugin.Log.LogInfo("Restoring proxy cards");
        //     Plugin.proxyTrials.ForEach(x => CustomTarotCardManager.CustomTarotCardList.Add(x.Key, x.Value));
        //     Plugin.proxyAugments.ForEach(x => CustomTarotCardManager.CustomTarotCardList.Add(x.Key, x.Value));
        // }

        //HealthPlayer.DealDamage: the player takes damage. patches for trials 13 and 14, and augment 1
        //(1) patch HealthPlayer.DealDamage TrapPoison.CreatePoison(base.transform.position, 1, 0f, GenerateRoom.Instance.transform, false);
        //(13) patch 1: HealthPlayer.DealDamage if player is hit, for each active trial, 10% chance to lose tarot card, if no tarot card, lose 1 max hp, patch 2: Health.DealDamage if player is hit, for each active augment, lose 10% curse charge
        //(14) patch 1: HealthPlayer.DealDamage if player is hit, for each active trial, all enemies heal 10% hp,
        [HarmonyPatch(typeof(HealthPlayer), nameof(HealthPlayer.DealDamage))]
        [HarmonyPostfix]
        public static void HealthPlayer_DealDamage_Post(ref bool __result, HealthPlayer __instance, GameObject Attacker, Vector3 AttackLocation)
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

                    var enemies = GameObject.FindObjectsOfType<UnitObject>();
                    foreach (var enemy in enemies)
                    {
                        if (enemy.health.team == Health.Team.Team2)
                        {
                            enemy.health.Heal(enemy.health.totalHP * healPercent);
                        }
                    }

                }

            }
        }

        //(10) patch PlayerWeapon.DoAttackRoutine spawn 2 bombs around player
        //(6) patch Health.DealDamage if Enemy team, 25% chance to ignore damage
        [HarmonyPatch(typeof(Health), nameof(Health.DealDamage))]
        [HarmonyPostfix]
        public static void Health_DealDamage_post(Health __instance, GameObject Attacker, Vector3 AttackLocation)
        {
            if (__instance.team != Health.Team.Team2) return;

            //(10) patch PlayerWeapon.DoAttackRoutine spawn 2 bombs around player
            if (Plugin.proxyAugmentsEnabled.Contains(Plugin.BombardmentCard))
            {
                Plugin.Log.LogInfo("Augment of Bombardment triggered");
                Bomb.CreateBomb(AttackLocation, __instance, Attacker.transform.parent);
                Bomb.CreateBomb(AttackLocation, __instance, Attacker.transform.parent);
                AudioManager.Instance.PlayOneShot("event:/boss/spider/bomb_shoot", Attacker.transform.position);
            }

            //(14) patch 2: Health.DealDamage if enemy is hit, for each active augment, enemy gains 0.5% damage reduction
            if (Plugin.proxyTrialsEnabled.Contains(Plugin.ShamuraCard))
            {
                Plugin.Log.LogInfo("Trial of Shamura (Augment) triggered, enemy gains damage reduction");
                var augmentDamage = 0.005f * Plugin.proxyAugmentsEnabled.Count;
                __instance.DamageModifier = Mathf.Clamp(__instance.DamageModifier - augmentDamage, 0.1f, 1000f);
            }


        }

        //(12) patch 1: Health.DealDamage if non lethal damage to enemy, 5% chance to duplicate at same health before taking damage per active augment,
        //(11) patch 2: HealthPlayer.DealDamage if enemy hits player, 10% chance to insta kill per active trial

        [HarmonyPatch(typeof(Health), nameof(Health.DealDamage))]
        [HarmonyPrefix]
        public static bool Health_DealDamage_pre(ref bool __result, Health __instance, GameObject Attacker, Vector3 AttackLocation)
        {
            if (__instance.team == Health.Team.PlayerTeam)
            {
                //(11) patch 2: HealthPlayer.DealDamage if enemy hits player, 10% chance to insta kill per active trial
                if (Plugin.proxyTrialsEnabled.Contains(Plugin.NarinderCard))
                {
                    var chance = 10 * Plugin.proxyTrialsEnabled.Count;
                    if (Random.Range(0, 100) < chance)
                    {
                        Plugin.Log.LogInfo("Trial of Narinder (Trial) triggered: execute player");
                        __instance.DamageModifier = 9999f;
                        __result = true;
                        return false;
                    }
                }

            }

            if (__instance.team == Health.Team.Team2)
            {
                //(6) patch Health.DealDamage if Enemy team, 25% chance to ignore damage
                if (Plugin.proxyAugmentsEnabled.Contains(Plugin.ResistanceCard))
                {
                    if (Random.Range(0, 100) < 25)
                    {
                        Plugin.Log.LogInfo("Augment of Resistance triggered on " + __instance.name);
                        __result = false;
                        return false;
                    }
                }

                //(12) patch 1: Health.DealDamage if non lethal damage to enemy, 5% chance to duplicate at same health before taking damage per active augment, 
                if (Plugin.proxyTrialsEnabled.Contains(Plugin.LeshyCard))
                {
                    var enemies = GameObject.FindObjectsOfType<UnitObject>();
                    if (enemies.Length >= 20)
                    {
                        Plugin.Log.LogInfo("Trial of Leshy (Augment) skipped, too many enemies to spawn duplicates");
                    }
                    else
                    {
                        var chance = 5 * Plugin.proxyAugmentsEnabled.Count;
                        if (Random.Range(0, 100) < chance)
                        {
                            Plugin.Log.LogInfo("Trial of Leshy (Augment) triggered on " + __instance.name);
                            //make a copy of this mob
                            var newMob = GameObject.Instantiate(__instance.gameObject, __instance.transform.parent);
                        }
                    }

                }
            }
            return true;
        }

        public static IEnumerator LeshySpawnDuplicateTrial()
        {
            while (true)
            {
                yield return new WaitForSeconds(10f);
                var enemies = GameObject.FindObjectsOfType<UnitObject>();
                if (enemies.Length >= 20)
                {
                    Plugin.Log.LogInfo("Trial of Leshy (Trial) skipped, too many enemies to spawn duplicates");
                    continue;
                }

                foreach (var unit in enemies)
                {
                    if (unit.health.team != Health.Team.Team2) continue;
                    var triggerTimes = Plugin.proxyTrialsEnabled.Count;
                    for (var i = 0; i < triggerTimes; i++)
                    {
                        Plugin.Log.LogInfo("Trial of Leshy (Trial) triggered on " + unit.name);
                        var newMob = GameObject.Instantiate(unit.gameObject, unit.transform.parent);
                    }
                }
            }
        }

        //(2) patch UnitObject.maxSpeed *= 2; UnitObject.SpeedMultiplier *= 2;
        //(5) patch Enemy.Update heal 5% per 3s, if boss then 1% per second (UnitObject.isBoss)
        //(15) patch 1: UnitObject.OnEnable every 6 - 1 per trial seconds, TrapPoison.CreatePoison
        //(11) patch 1: UnitObject.OnEnable add 50% max hp and heal to full per active augment,
        //(11) patch 3: UnitObject.OnEnable if UnitObject.isBoss, boss gains 5% hp per enemy kill,
        [HarmonyPatch(typeof(UnitObject), nameof(UnitObject.OnEnable))]
        [HarmonyPostfix]
        public static void UnitObject_OnEnable(UnitObject __instance)
        {
            if (__instance.health.team != Health.Team.Team2)
                return;

            //(2) patch UnitObject.maxSpeed *= 2; UnitObject.SpeedMultiplier *= 2;
            if (Plugin.proxyAugmentsEnabled.Contains(Plugin.SwarmCard))
            {
                Plugin.Log.LogInfo("Augment of Swarm triggered on " + __instance.name);
                __instance.maxSpeed *= 1.2f;
                __instance.SpeedMultiplier *= 1.2f;
            }



            //(11) patch 1: UnitObject.OnEnable add 50% max hp and heal to full per active augment,
            //(11) patch 3: UnitObject.OnEnable if UnitObject.isBoss, boss gains 5% hp per enemy kill,
            if (Plugin.proxyTrialsEnabled.Contains(Plugin.NarinderCard))
            {
                Plugin.Log.LogInfo("Trial of Narinder (Augment) triggered on " + __instance.name);
                __instance.health.totalHP *= 1.5f;
                __instance.health.Heal(__instance.health.totalHP);

                if (__instance.isBoss)
                {
                    var totalHealthBuff = 0.05f * Plugin.proxyTrialsEnabled.Count * killCount;
                    __instance.health.totalHP *= 1f + totalHealthBuff;
                    __instance.health.Heal(__instance.health.totalHP);
                    Plugin.Log.LogInfo("Trial of Narinder (Trial) triggered on Boss " + __instance.name + " now hp " + __instance.health.totalHP);
                }
            }
        }

        //(5) patch Enemy.Update heal 5% per 3s, if boss then 1% per second (UnitObject.isBoss)
        public static IEnumerator HealthRegen()
        {
            while (true)
            {
                yield return new WaitForSeconds(3f);
                var enemies = GameObject.FindObjectsOfType<UnitObject>();
                foreach (var unit in enemies)
                {
                    if (unit.health.team != Health.Team.Team2) continue;
                    Plugin.Log.LogInfo("Healing " + unit.name);
                    unit.health.Heal(unit.health.totalHP * (unit.isBoss ? 0.03f : 0.05f));
                }
            }
        }

        //(8) patch PlayerWeapon.DoAttackRoutine take 5% curse charge faithammo.ammo -
        [HarmonyPatch(typeof(PlayerWeapon), nameof(PlayerWeapon.DoAttackRoutine))]
        [HarmonyPostfix]
        public static void PlayerWeapon_DoAttackRoutine(PlayerWeapon __instance)
        {
            if (Plugin.proxyAugmentsEnabled.Contains(Plugin.CurseCard))
            {
                __instance.playerFarming.playerSpells.faithAmmo.Ammo -= __instance.playerFarming.playerSpells.faithAmmo.Total * 0.05f;
            }
        }

        //Enemy OnDie augment 3,4,9, trial 1
        //(3) patch Enemy.OnDie explode
        //(4) patch Enemy.OnDie give all other enemies +50% dmg
        //(9) patch Enemy.OnDie double hp of all other enemies
        //(11) Enemy.OnDie if not last enemy, 5% chance to respawn
        [HarmonyPatch(typeof(UnitObject), nameof(UnitObject.OnDie))]
        [HarmonyPostfix]
        public static void UnitObject_OnDie(UnitObject __instance)
        {
            if (__instance.health.team != Health.Team.Team2)
                return;

            killCount++;

            //(3) patch Enemy.OnDie explode
            if (Plugin.proxyAugmentsEnabled.Contains(Plugin.ExplosionCard))
            {
                Plugin.Log.LogInfo("Augment of Explosion triggered on " + __instance.name);
                Explosion.CreateExplosion(__instance.transform.position, Health.Team.Team2, __instance.health, 2f, 1f, 0f);
            }

            //(9) patch Enemy.OnDie double hp of all other enemies
            if (Plugin.proxyAugmentsEnabled.Contains(Plugin.GraceCard))
            {
                Plugin.Log.LogInfo("Augment of Grace triggered on " + __instance.name);
                var enemies = GameObject.FindObjectsOfType<UnitObject>();
                foreach (var enemy in enemies)
                {
                    if (enemy == __instance)
                        continue;

                    if (enemy.health.team != Health.Team.Team2)
                        continue;

                    var hp = enemy.health;
                    if (hp.HP > 0)
                    {
                        hp.totalHP *= 1.4f;
                        hp.Heal(hp.totalHP * 0.25f);
                    }
                }
            }

            //(4) patch Enemy.OnDie give all other enemies +10% dmg
            if (Plugin.proxyAugmentsEnabled.Contains(Plugin.BloodpactCard))
            {
                var enemies = GameObject.FindObjectsOfType<UnitObject>();
                Plugin.Log.LogInfo("Augment of Bloodpact triggered on " + __instance.name);
                foreach (var player in PlayerFarming.players)
                {
                    player.health.DamageModifier += 0.1f;
                }
            }

        }

        //(7) patch Playerfarming.DodgeRoll PRE: check if curse charge is at least 20%, cannot roll without curse charge. POST: if result is true, take 20% curse charge
        [HarmonyPatch(typeof(PlayerFarming), nameof(PlayerFarming.DodgeRoll))]
        [HarmonyPrefix]
        public static bool PlayerFarming_DodgeRoll_Pre(PlayerFarming __instance, ref bool __result)
        {
            if (Plugin.proxyAugmentsEnabled.Contains(Plugin.DissonanceCard))
            {
                if (__instance.playerSpells.faithAmmo.Ammo < (0.2f * __instance.playerSpells.faithAmmo.Total))
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(PlayerFarming), nameof(PlayerFarming.DodgeRoll))]
        [HarmonyPostfix]
        public static void PlayerFarming_DodgeRoll_Post(PlayerFarming __instance, ref bool __result)
        {
            if (Plugin.proxyAugmentsEnabled.Contains(Plugin.DissonanceCard) && __result)
            {
                Plugin.Log.LogInfo("Augment of Dissonance triggered");
                __instance.playerSpells.faithAmmo.Ammo = Mathf.Clamp(__instance.playerSpells.faithAmmo.Ammo - (__instance.playerSpells.faithAmmo.Total * 0.2f), 0, __instance.playerSpells.faithAmmo.Total);
            }
        }

        //(12) patch 2: GenerateRoom.Generate postfix instantiate copy of each enemy per active trial
        // [HarmonyPatch(typeof(GenerateRoom), nameof(GenerateRoom.Generate), [])]
        // [HarmonyPostfix]
        // public static IEnumerator GenerateRoom_Generate(IEnumerator result, GenerateRoom __instance)
        // {
        //     while (result.MoveNext())
        //     {
        //         yield return result.Current;
        //     }

        //     if (Plugin.proxyAugmentsEnabled.Contains(Plugin.LeshyCard))
        //     {
        //         Plugin.Log.LogInfo("Trial of Leshy (Trial) triggered " + __instance.name);
        //         var enemies = GameObject.FindObjectsOfType<UnitObject>();
        //         foreach (var enemy in enemies)
        //         {
        //             if (enemy.health.HP > 0)
        //             {
        //                 for (var i = 0; i < Plugin.proxyTrialsEnabled.Count; i++)
        //                 {
        //                     Plugin.Log.LogInfo("New Copy of " + enemy.name);
        //                     var newEnemy = GameObject.Instantiate(enemy.gameObject, enemy.transform.position, enemy.transform.rotation);
        //                 }
        //             }
        //         }
        //     }
        // }


    }
}
