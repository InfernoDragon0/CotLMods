using COTL_API.Structures;
using HarmonyLib;
using Lamb.UI;
using Lamb.UI.BuildMenu;
using Lamb.UI.FollowerInteractionWheel;
using MMTools;
using System;
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

        [HarmonyPatch(typeof(StructuresData), "GetUnlocked")] //temporary unlock
        [HarmonyPrefix]
        private static void StructuresData_GetUnlocked(StructureBrain.TYPES Types)
        {
            if (!DataManager.Instance.UnlockedStructures.Contains(Types)) DataManager.Instance.UnlockedStructures.Add(Types);
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
            FollowerCommandItems.FollowerRoleCommandItem followerRoleCommandItem = new FollowerCommandItems.FollowerRoleCommandItem();
            followerRoleCommandItem.Command = FollowerCommands.Cook_2;
            followerRoleCommandItem.FollowerTaskType = FollowerTaskType.Cook;
            Plugin.Log.LogInfo(followerRoleCommandItem);
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

        //Enable devotion progress as cooking progress bar (there is no progress bar for the chef in vanilla)
        [HarmonyPatch(typeof(FollowerTask_Cook), "OnDoingBegin")]
        [HarmonyPrefix]
        private static void FollowerTask_Cook_OnDoingBegin(Follower follower)
        {
            follower1 = follower;
            follower1.UIFollowerPrayingProgress.Show();
        }

        //Enable devotion progress as cooking progress bar (there is no progress bar for the chef in vanilla)
        [HarmonyPatch(typeof(FollowerTask_Cook), "TaskTick")]
        [HarmonyPrefix]
        private static bool FollowerTask_Cook_TaskTick(FollowerTask_Cook __instance, float deltaGameTime)
        {
            if (__instance.State != FollowerTaskState.Doing || (__instance.kitchenStructure.Data.QueuedMeals.Count <= 0 && __instance.kitchenStructure.Data.CurrentCookingMeal == null))
                return false;

            if (__instance.kitchenStructure.Data.CurrentCookingMeal == null)
                __instance.kitchenStructure.Data.CurrentCookingMeal = __instance.kitchenStructure.Data.QueuedMeals[0];

            else if (__instance.kitchenStructure.Data.CurrentCookingMeal.CookedTime >= __instance.kitchenStructure.Data.CurrentCookingMeal.CookingDuration)
            {
                if (follower1 != null)
                {
                    follower1.TimedAnimation("Reactions/react-laugh", 3.33f, (System.Action)(() => __instance.ProgressTask()));
                    follower1.UIFollowerPrayingProgress.Flash();
                }
                __instance.MealFinishedCooking();

            }
            
            else
                __instance.kitchenStructure.Data.CurrentCookingMeal.CookedTime += deltaGameTime * __instance._brain.Info.ProductivityMultiplier;

            return false;
        }

        //Enable devotion progress as cooking progress bar (there is no progress bar for the chef in vanilla)
        [HarmonyPatch(typeof(FollowerTask_Cook), "Cleanup")]
        [HarmonyPrefix]
        private static void FollowerTask_Cook_Cleanup(Follower follower)
        {
            Plugin.Log.LogInfo("Follower has been cleaned up");
            follower1 = null;
            follower.UIFollowerPrayingProgress.Hide();

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

    }
}
