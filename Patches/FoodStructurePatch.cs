using HarmonyLib;
using Lamb.UI;
using Lamb.UI.BuildMenu;
using Lamb.UI.FollowerInteractionWheel;
using MMTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CotLTemplateMod.Patches
{
    [HarmonyPatch]
    internal class FoodStructurePatch
    {
        public static Follower follower1; //temp follower storage for the kitchen
        private static Coroutine _dissentBubbleCoroutine; //for the bubble


        [HarmonyPatch(typeof(FollowerCategory), "GetStructuresForCategory")]
        [HarmonyPostfix]
        private static void FollowerCategory_GetStructuresForCategory(ref List<StructureBrain.TYPES> __result, FollowerCategory.Category category)
        {
            if (category == FollowerCategory.Category.Food)
            {
                Plugin.Log.LogInfo("Add food storage");
                __result.Add(StructureBrain.TYPES.FOOD_STORAGE);
                __result.Add(StructureBrain.TYPES.FOOD_STORAGE_2);
                __result.Add(StructureBrain.TYPES.KITCHEN);
                __result.Add(StructureBrain.TYPES.KITCHEN_II);
            }
        }

        [HarmonyPatch(typeof(StructuresData), "GetUnlocked")] //unlock kitchen and food storage structures
        [HarmonyPrefix]
        private static void StructuresData_GetUnlocked(StructureBrain.TYPES Types)
        {
            if (!DataManager.Instance.UnlockedStructures.Contains(Types) && (Types == StructureBrain.TYPES.KITCHEN || Types == StructureBrain.TYPES.KITCHEN_II || Types ==  StructureBrain.TYPES.FOOD_STORAGE || Types == StructureBrain.TYPES.FOOD_STORAGE_2))
            {
                DataManager.Instance.UnlockedStructures.Add(Types);
            }
            if (!DataManager.Instance.RevealedStructures.Contains(Types)) DataManager.Instance.RevealedStructures.Add(Types);
        }

        [HarmonyPatch(typeof(UpgradeSystem), "GetUnlocked")] //unlock kitchen and food storage
        [HarmonyPrefix]
        private static void UpgradeSystem_GetUnlocked(UpgradeSystem.Type Type)
        {
            if (!DataManager.Instance.UnlockedUpgrades.Contains(Type) && (Type == UpgradeSystem.Type.Building_FoodStorage || Type == UpgradeSystem.Type.Building_FoodStorage2 || Type == UpgradeSystem.Type.Building_Kitchen || Type == UpgradeSystem.Type.Building_KitchenII))
            {
                DataManager.Instance.UnlockedUpgrades.Add(Type);
            }
        }

        [HarmonyPatch(typeof(StructuresData), "GetCost")]
        [HarmonyPrefix] //temporary override cost
        private static bool StructuresData_GetCost(ref List<StructuresData.ItemCost> __result, StructureBrain.TYPES Type)
        {
            if (Type == StructureBrain.TYPES.FOOD_STORAGE || Type == StructureBrain.TYPES.KITCHEN)
            {
                __result = new List<StructuresData.ItemCost>()
                            {
                              new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.LOG, 1)
                            };
                return false;
            }
            return true;
            
        }
        //Add the task for follower to cook
        [HarmonyPatch(typeof(FollowerBrain), "IsTaskAvailable")]
        [HarmonyPostfix]
        private static void FollowerBrain_IsTaskAvailable(ref bool __result, FollowerTaskType taskType)
        {
            if (taskType == FollowerTaskType.Cook)
            {
                __result = true;
            }

        }


        //Add the task for follower to cook
        [HarmonyPatch(typeof(FollowerCommandGroups), "GiveWorkerCommands")]
        [HarmonyPostfix]
        private static void FollowerCommandGroups_GiveWorkerCommands(ref List<CommandItem> __result)
        {
            if (!Plugin.chefJob.Value) return;
            FollowerCommandItems.FollowerRoleCommandItem followerRoleCommandItem = new FollowerCommandItems.FollowerRoleCommandItem();
            followerRoleCommandItem.Command = FollowerCommands.Cook_2;
            followerRoleCommandItem.FollowerTaskType = FollowerTaskType.Cook;

            
            __result.Add(followerRoleCommandItem);

        }


        //hard swap to task
        [HarmonyPatch(typeof(interaction_FollowerInteraction),
        nameof(interaction_FollowerInteraction.OnFollowerCommandFinalized))]
        [HarmonyPrefix]
        private static bool interaction_FollowerInteraction_OnFollowerCommandFinalized(
        interaction_FollowerInteraction __instance, FollowerCommands[] followerCommands)
        {
            FollowerCommands command = followerCommands[0];
            FollowerCommands preFinalCommand = followerCommands.Length > 1 ? followerCommands[1] : FollowerCommands.None;

            if (command == FollowerCommands.Cook_2)
            {
                Plugin.Log.LogInfo("time to cook");
                __instance.follower.Brain.NewRoleSet(FollowerRole.Chef);
                __instance.follower.Brain.SetPersonalOverrideTask(FollowerTask.GetFollowerTaskFromRole(FollowerRole.Chef));
                __instance.StartCoroutine(__instance.FrameDelayCallback((System.Action)(() =>
                {
                    __instance.follower.Brain.Info.FollowerRole = FollowerRole.Chef;
                    __instance.follower.Brain.Info.Outfit = FollowerOutfitType.Follower;
                    __instance.follower.SetOutfit(FollowerOutfitType.Follower, false);

                    if (Interaction_Kitchen.Kitchens.Count > 0)
                    {
                        Plugin.Log.LogInfo("got kitchen");
                        FollowerTask_Cook cook = new FollowerTask_Cook(Interaction_Kitchen.Kitchens[0].structure.Structure_Info.ID);
                        __instance.follower.Brain.HardSwapToTask(cook);
                    }

                })));
                __instance.Close();
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(FollowerTask_Cook), "Setup")]
        [HarmonyPostfix]
        private static void FollowerTask_Cook_Setup(Follower follower)
        {
            follower1 = follower;
            _dissentBubbleCoroutine = follower.StartCoroutine(DissentBubbleRoutine(follower));
        }

        //Enable cooking indicator (there is no progress bar for the chef in vanilla)
        [HarmonyPatch(typeof(FollowerTask_Cook), "OnDoingBegin")]
        [HarmonyPrefix]
        private static void FollowerTask_Cook_OnDoingBegin(Follower follower) //TODO change to worshipperbubble "food"?
        {
            follower1 = follower;
            if (_dissentBubbleCoroutine == null && follower1 != null)
                _dissentBubbleCoroutine = follower1.StartCoroutine(DissentBubbleRoutine(follower1));
        }

        //Enable devotion progress as cooking progress bar (there is no progress bar for the chef in vanilla)
        [HarmonyPatch(typeof(FollowerTask_Cook), "TaskTick")]
        [HarmonyPrefix]
        private static bool FollowerTask_Cook_TaskTick(FollowerTask_Cook __instance, float deltaGameTime)
        {
            if (__instance.State != FollowerTaskState.Doing || (__instance.kitchenStructure.Data.QueuedMeals.Count <= 0 && __instance.kitchenStructure.Data.CurrentCookingMeal == null))
            {
                //Plugin.Log.LogInfo("Closing bubble");
                if (_dissentBubbleCoroutine != null)
                {
                    follower1.WorshipperBubble.StopCoroutine(_dissentBubbleCoroutine);
                    _dissentBubbleCoroutine = null;
                    follower1.WorshipperBubble.Close();
                    Plugin.Log.LogInfo("closed bubble");
                }
                return false;

            }

            if (__instance.kitchenStructure.Data.CurrentCookingMeal == null)
                __instance.kitchenStructure.Data.CurrentCookingMeal = __instance.kitchenStructure.Data.QueuedMeals[0];

            else if (__instance.kitchenStructure.Data.CurrentCookingMeal.CookedTime >= __instance.kitchenStructure.Data.CurrentCookingMeal.CookingDuration)
            {
                if (follower1 != null)
                {
                    follower1.TimedAnimation("Reactions/react-laugh", 3.33f, (System.Action)(() => __instance.ProgressTask()));
                }
                __instance.MealFinishedCooking();

            }
            
            else
            {
                if (_dissentBubbleCoroutine == null && follower1 != null)
                {
                    _dissentBubbleCoroutine = follower1.StartCoroutine(DissentBubbleRoutine(follower1));
                    follower1.SetBodyAnimation("action", true);

                }

                __instance.kitchenStructure.Data.CurrentCookingMeal.CookedTime += deltaGameTime * __instance._brain.Info.ProductivityMultiplier;

            }

            return false;
        }

        //Enable devotion progress as cooking progress bar (there is no progress bar for the chef in vanilla)
        [HarmonyPatch(typeof(FollowerTask_Cook), "Cleanup")]
        [HarmonyPrefix]
        private static void FollowerTask_Cook_Cleanup(Follower follower)
        {
            Plugin.Log.LogInfo("Follower has been cleaned up");
            if (_dissentBubbleCoroutine != null)
            {
                follower.WorshipperBubble.StopCoroutine(_dissentBubbleCoroutine);
                _dissentBubbleCoroutine = null;
                follower.WorshipperBubble.Close();
            }
            follower1 = null;


        }

        //Add blocking task (temporary)
        [HarmonyPatch(typeof(FollowerTask), "BlockTaskChanges", MethodType.Getter)]
        [HarmonyPostfix]
        private static void FollowerTask_Cook_BlockTaskChanges(FollowerTask __instance, ref bool __result)
        {
            if (__instance.Type == FollowerTaskType.Cook)
            {
                __result = true;
            }
        }

        //cooking isnt a priority task so the chef will leave every time they finish one meal without this fix
        [HarmonyPatch(typeof(FollowerTask_Cook),nameof(FollowerTask_Cook.MealFinishedCooking))]
        [HarmonyPrefix]
        private static bool FollowerTask_Cook_MealFinishedCookingFix(FollowerTask_Cook __instance)
        {
            ++DataManager.Instance.MealsCooked;
            ObjectiveManager.CheckObjectives(Objectives.TYPES.COOK_MEALS);
            /*Interaction_Kitchen kitchen = __instance.FindKitchen();
            if (kitchen != null)
            {
                kitchen.MealFinishedCooking();
            }*/
            /*else
            {*/
                Structures_FoodStorage availableFoodStorage = Structures_FoodStorage.GetAvailableFoodStorage(__instance.kitchenStructure.Data.Position, __instance.Location);
                if (availableFoodStorage != null)
                {
                    availableFoodStorage.DepositItemUnstacked(__instance.kitchenStructure.Data.CurrentCookingMeal.MealType);
                }
                else
                {
                    Plugin.Log.LogInfo("No Food Storage?");
                    StructureBrain.TYPES mealStructureType = StructuresData.GetMealStructureType(__instance.kitchenStructure.Data.CurrentCookingMeal.MealType);
                    Vector3 position = __instance.kitchenStructure.Data.Position + (Vector3)Random.insideUnitCircle * 2f;
                    StructureManager.BuildStructure(__instance.kitchenStructure.Data.Location, StructuresData.GetInfoByType(mealStructureType, 0), position, Vector2Int.one);
                }
                CookingData.CookedMeal(__instance.kitchenStructure.Data.CurrentCookingMeal.MealType);
                ObjectiveManager.CheckObjectives(Objectives.TYPES.COOK_MEALS);
                __instance.kitchenStructure.Data.QueuedMeals.Remove(__instance.kitchenStructure.Data.CurrentCookingMeal);
                __instance.kitchenStructure.Data.CurrentCookingMeal = null;
                //__instance.kitchenStructure.Data.Fuel -= 10;
            /*}*/
            if (__instance.kitchenStructure.Data.QueuedMeals.Count <= 0)
            {
                Plugin.Log.LogInfo("no more food to cook, waiting for new food");
                //__instance.Complete();
            }
                

            return false;
        }

        //All you can eat buffet
        [HarmonyPatch(typeof(Meal), "EatRoutine", MethodType.Enumerator)]
        [HarmonyPostfix]
        private static void Meal_EatRoutine_EatMore()
        {
            if (!Plugin.playerEatMore.Value) return;
            DataManager.instance.PlayerEaten = false;
            Plugin.Log.LogInfo("now you can eat more!");
        }

        //(non vanilla) Kitchen reduces the cost of recipes by 50%, rounded down, minimum cost 1
        [HarmonyPatch(typeof(CookingData), "GetRecipe")]
        [HarmonyPostfix]
        private static void CookingData_RecipeDiscount(ref List<List<InventoryItem>> __result)
        {
            if (!Plugin.kitchenDiscount.Value) return;
            if (Interaction_Kitchen.Kitchens.Count == 0) return;
            
            if (Interaction_Kitchen.Kitchens[0].StructureInfo.Type == StructureBrain.TYPES.KITCHEN)
            {
                foreach (List<InventoryItem> data1 in __result) //it do be like that
                {
                    foreach (InventoryItem data2 in data1)
                    {
                        data2.quantity = (data2.quantity / 2) < 1 ? 1 : (data2.quantity / 2);
                    }
                }
                //Plugin.Log.LogInfo("edited prices");
            }
            else
            {
                Plugin.Log.LogInfo("kitchen found but not a kitchen, is a " + Interaction_Kitchen.Kitchens[0].StructureInfo.Type.ToString());
            }
            
        }

        //Move the chef backwards from the pot
        [HarmonyPatch(typeof(FollowerTask_Cook), "UpdateDestination")]
        [HarmonyPostfix]
        private static void FollowerTask_Cook_UpdateDestination(FollowerTask_Cook __instance, ref Vector3 __result)
        {
            __result = __instance.kitchenStructure.Data.Position + new Vector3(0.0f, 2.521f);
        }

        //Let the waiter serve
        [HarmonyPatch(typeof(FollowerBrain), "CheckEatTask")]
        [HarmonyPostfix]
        private static void FollowerBrain_CheckEatTask(ref FollowerTask __result)
        {
            if (!Plugin.waiterJob.Value) return;
            __result = null;
        }

        private static IEnumerator DissentBubbleRoutine(Follower follower) //for the bubble
        {
            float bubbleTimer = 0.3f;
            while (true)
            {
                if ((double)(bubbleTimer -= Time.deltaTime) < 0.0)
                {
                    WorshipperBubble.SPEECH_TYPE Type = WorshipperBubble.SPEECH_TYPE.FOOD;
                    follower.WorshipperBubble.gameObject.SetActive(true);
                    follower.WorshipperBubble.Play(Type);
                    bubbleTimer = (4 + UnityEngine.Random.Range(0, 2));
                }
                yield return null;
            }
        }

    }
}
