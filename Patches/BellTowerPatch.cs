using HarmonyLib;
using I2.Loc;
using MMTools;
using Socket.Newtonsoft.Json.Utilities.LinqBridge;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CotLTemplateMod.Patches
{
    [HarmonyPatch]

    internal class BellTowerPatch
    {
        //TODO: do something about the bell tower, maybe pay 5 gold to "bless" the food by increasing satiation x2 "BellTower.cs"

        //TODO: bell tower will start a animation on the character, after a few seconds, while loop the total amount of food and collect with wait x seconds
        //Then end animation, and fill the hunger bar by all the food used x2

        // This patch edits the label for belltower
        [HarmonyPatch(typeof(BellTower), nameof(BellTower.GetLabel))]
        [HarmonyPostfix]
        public static void BellTower_GetLabel(BellTower __instance)
        {
            __instance.Label = "10 Gold: Convert Food to 2x Hunger (No Perks)";
        }

        // This patch edits bell interaction
        [HarmonyPatch(typeof(BellTower), nameof(BellTower.OnInteract))]
        [HarmonyPrefix]
        public static bool BellTower_OnInteract(BellTower __instance, StateMachine state)
        {
            //collect the loot
            if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD) < 10)
            {
                return false;
            }

            Inventory.ChangeItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD, -10);
            ResourceCustomTarget.Create(PlayerFarming.Instance.gameObject, __instance.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);

            __instance.StartCoroutine(BlessRoutine());
            return false;
        }

        //this is just fancy animations for the lamb
        public static IEnumerator<object> BlessRoutine()
        {
            GameManager.GetInstance().OnConversationNew();
            GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone);
            AudioManager.Instance.PlayOneShot("event:/sermon/start_sermon", PlayerFarming.Instance.gameObject);
            AudioManager.Instance.PlayOneShot("event:/building/building_bell_ring", PlayerFarming.Instance.gameObject);

            PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
            PlayerFarming.Instance.Spine.UseDeltaTime = false;
            PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
            PlayerFarming.Instance.Spine.UseDeltaTime = false;
            PlayerFarming.Instance.Spine.skeleton.FindBone("ritualring").Rotation += 60f;
            PlayerFarming.Instance.Spine.skeleton.UpdateWorldTransform();
            PlayerFarming.Instance.Spine.skeleton.Update(Time.deltaTime);
            PlayerFarming.Instance.simpleSpineAnimator.Animate("sermons/sermon-start", 0, false);
            PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("sermons/sermon-loop", 0, true, 0.0f);

            yield return new WaitForSeconds(3f);

            //loop all the food here
            foreach (Structures_Meal structuresMeal in StructureManager.GetAllStructuresOfType<Structures_Meal>(FollowerLocation.Base))
            {
                if (!structuresMeal.Data.Rotten && !structuresMeal.Data.Burned && !structuresMeal.ReservedForTask)
                {
                    ResourceCustomTarget.Create(PlayerFarming.Instance.gameObject, structuresMeal.Data.Position, InventoryItem.ITEM_TYPE.MEAL, null);

                    FollowerBrain fb = FollowerManager.GetHungriestFollowerBrain();
                    fb.Stats.Satiation += CookingData.GetSatationAmount(CookingData.GetMealFromStructureType(structuresMeal.Data.Type)) * 2;
                    structuresMeal.Data.Eaten = true;
                    yield return new WaitForSeconds(0.3f);
                }
            }

            //loop all the food storage here
            foreach (Structures_FoodStorage structuresFoodStorage in StructureManager.GetAllStructuresOfType<Structures_FoodStorage>(PlayerFarming.Location))
            {
                foreach (InventoryItem inventoryItem in structuresFoodStorage.Data.Inventory.ToList()) //inefficient list things but its not that bad
                {
                    if (inventoryItem.UnreservedQuantity > 0)
                    {
                        structuresFoodStorage.TryClaimFoodReservation((InventoryItem.ITEM_TYPE)inventoryItem.type);
                        if (structuresFoodStorage.TryEatReservedFood((InventoryItem.ITEM_TYPE)inventoryItem.type))
                        {
                            ResourceCustomTarget.Create(PlayerFarming.Instance.gameObject, structuresFoodStorage.Data.Position, (InventoryItem.ITEM_TYPE)inventoryItem.type, null);
                            StructureBrain.TYPES mealStructureType = StructuresData.GetMealStructureType((InventoryItem.ITEM_TYPE)inventoryItem.type);
                            FollowerBrain fb = FollowerManager.GetHungriestFollowerBrain();
                            fb.Stats.Satiation += CookingData.GetSatationAmount(CookingData.GetMealFromStructureType(mealStructureType)) * 2;
                            yield return new WaitForSeconds(0.3f);
                            
                        }

                    }
                }

            }

            //exit from animation
            PlayerFarming.Instance.simpleSpineAnimator.Animate("sermons/sermon-stop", 0, false);
            PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0.0f);
            AudioManager.Instance.PlayOneShot("event:/sermon/end_sermon", PlayerFarming.Instance.gameObject);
            AudioManager.Instance.PlayOneShot("event:/sermon/book_put_down", PlayerFarming.Instance.gameObject);

            yield return new WaitForSeconds(2f);

            PlayerFarming.Instance.Spine.UseDeltaTime = true;
            PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;

            GameManager.GetInstance().OnConversationEnd();
        }

    }
}
