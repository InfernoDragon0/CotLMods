using System;
using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace CotLMiniMods.Patches {

    [HarmonyPatch]
    internal class Interaction_DrinkHouse_Patch
    {
        
        [HarmonyPatch(typeof(Interaction_ReserveDrink), (nameof(Interaction_ReserveDrink.Configure)))]
        [HarmonyPostfix]
        public static void Interaction_ReserveDrink_Configure(Interaction_ReserveDrink __instance)
        {
            __instance.HasSecondaryInteraction = true;
        }

        [HarmonyPatch(typeof(Interaction_ReserveDrink), (nameof(Interaction_ReserveDrink.GetLabel)))]
        [HarmonyPostfix]
        public static void Interaction_ReserveDrink_GetLabel(Interaction_ReserveDrink __instance)
        {
            __instance.Interactable = !Structures_Pub.IsDrinking && __instance.pub.Brain.FoodStorage.Data.Inventory.Count > __instance.seat && __instance.pub.Brain.FoodStorage.Data.Inventory[__instance.seat] != null;
            if (!__instance.Interactable)
                __instance.secondaryLabel = "";
            else
                __instance.secondaryLabel = "Drink " + __instance.DrinkName;
        }

        [HarmonyPatch(typeof(Interaction), (nameof(Interaction.OnSecondaryInteract)))]
        [HarmonyPostfix]
        public static void Interaction_ReserveDrink_OnSecondaryInteract(Interaction __instance) {
            if (__instance is Interaction_ReserveDrink) {
                Plugin.Log.LogInfo("Lamby drinks");
                Interaction_ReserveDrink drinkInstance = __instance as Interaction_ReserveDrink;
                __instance.StartCoroutine(DrinkAnimation());
                drinkInstance.pub.RemoveDrinkFromTable(drinkInstance.seat);
                drinkInstance.pub.structureBrain.FinishedDrink(drinkInstance.seat, drinkInstance.drink);
            }
            
        }

        //drink IEnumerator
        public static IEnumerator DrinkAnimation()
        {
            //DEBUG all spine animation
            Plugin.Log.LogInfo("All spine animation");
            PlayerFarming.Instance.Spine.skeleton.data.Animations.ForEach(x => Plugin.Log.LogInfo(x.Name));
            Plugin.Log.LogInfo("End All spine animation");
            GameManager.GetInstance().OnConversationNew();
            //drink animation
            PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
            PlayerFarming.Instance.Spine.UseDeltaTime = false;
            PlayerFarming.Instance.simpleSpineAnimator.Animate("drink-good", 0, false);
            PlayerFarming.Instance.Spine.UseDeltaTime = false;
            PlayerFarming.Instance.Spine.skeleton.Update(Time.deltaTime);

            yield return new WaitForSeconds(5.3f);

            AudioManager.Instance.PlayOneShot("event:/hearts_of_the_faithful/hearts_appear", PlayerFarming.Instance.Spine.gameObject);
            AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", PlayerFarming.Instance.Spine.gameObject.transform.position);
            if (!DataManager.Instance.SurvivalModeActive)
                BiomeConstants.Instance.EmitHeartPickUpVFX(PlayerFarming.Instance.CameraBone.transform.position, 0.0f, "red", "burst_big", (double) Time.timeScale == 1.0);


            yield return new WaitForSeconds(1f);
            HealthPlayer component = PlayerFarming.Instance.GetComponent<HealthPlayer>();
            component.BlueHearts += 2f;

            GameManager.GetInstance().OnConversationEnd();
            PlayerFarming.Instance.Spine.UseDeltaTime = true;
            yield return null;
        }
    }

}


