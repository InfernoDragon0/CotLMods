using COTL_API.CustomStructures;
using COTL_API.Helpers;
using CotLMiniMods.CCommands.Tasks;
using DG.Tweening;
using HarmonyLib;
using Lamb.UI;
using Lamb.UI.BuildMenu;
using Lamb.UI.FollowerInteractionWheel;
using MMTools;
using src.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace CotLMiniMods.Patches
{
    [HarmonyPatch]
    internal class FoodStructurePatch
    {
        public static Follower follower1; //temp follower storage for the kitchen
        private static Coroutine _dissentBubbleCoroutine; //for the bubble

        //There is Interaction_FoodStorage with no label, and add a interact to update


        [HarmonyPatch(typeof(FollowerCategory), "GetStructuresForCategory")]
        [HarmonyPostfix]
        private static void FollowerCategory_GetStructuresForCategory(ref List<StructureBrain.TYPES> __result, FollowerCategory.Category category)
        {
            if (category == FollowerCategory.Category.Food)
            {
                Plugin.Log.LogInfo("Add food storage and fishing");
                /*__result.Add(StructureBrain.TYPES.FOOD_STORAGE);
                __result.Add(StructureBrain.TYPES.FOOD_STORAGE_2);*/
               /* __result.Add(StructureBrain.TYPES.KITCHEN);
                __result.Add(StructureBrain.TYPES.KITCHEN_II);*/
                //__result.Add(StructureBrain.TYPES.FISHING_SPOT);
                __result.Add(StructureBrain.TYPES.FISHING_HUT);
                //__result.Add(StructureBrain.TYPES.FISHING_HUT_2);
            }
        }

        [HarmonyPatch(typeof(StructuresData), "GetUnlocked")] //unlock kitchen and food storage structures
        [HarmonyPrefix]
        private static void StructuresData_GetUnlocked(StructureBrain.TYPES Types)
        {
            if (!DataManager.Instance.UnlockedStructures.Contains(Types) && (Types == StructureBrain.TYPES.KITCHEN /*|| Types == StructureBrain.TYPES.KITCHEN_II*/ || Types ==  StructureBrain.TYPES.FOOD_STORAGE || Types == StructureBrain.TYPES.FOOD_STORAGE_2 || Types == StructureBrain.TYPES.FISHING_HUT /*|| Types == StructureBrain.TYPES.FISHING_HUT_2*/))
            {
                DataManager.Instance.UnlockedStructures.Add(Types);
            }
            if (!DataManager.Instance.RevealedStructures.Contains(Types)) DataManager.Instance.RevealedStructures.Add(Types);
        }

        [HarmonyPatch(typeof(UpgradeSystem), "GetUnlocked")] //unlock kitchen and food storage
        [HarmonyPrefix]
        private static void UpgradeSystem_GetUnlocked(UpgradeSystem.Type Type)
        {
            if (!DataManager.Instance.UnlockedUpgrades.Contains(Type) && (Type == UpgradeSystem.Type.Building_FoodStorage || Type == UpgradeSystem.Type.Building_FoodStorage2 || Type == UpgradeSystem.Type.Building_Kitchen || Type == UpgradeSystem.Type.Building_KitchenII || Type == UpgradeSystem.Type.Building_FishingHut2))
            {
                DataManager.Instance.UnlockedUpgrades.Add(Type);
            }
        }

        [HarmonyPatch(typeof(StructuresData), "GetCost")]
        [HarmonyPrefix] //temporary override cost
        private static bool StructuresData_GetCost(ref List<StructuresData.ItemCost> __result, StructureBrain.TYPES Type)
        {
            if (Type == StructureBrain.TYPES.FOOD_STORAGE || Type == StructureBrain.TYPES.KITCHEN || Type == StructureBrain.TYPES.FISHING_HUT_2)
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
                        FollowerTask_CookCustom cook = new FollowerTask_CookCustom(Interaction_Kitchen.Kitchens[0].structure.Structure_Info.ID);
                        __instance.follower.Brain.HardSwapToTask(cook);
                    }

                })));
                __instance.Close();
                return false;
            }
            return true;
        }

        /*[HarmonyPatch(typeof(FollowerTask_Cook), "Setup")]
        [HarmonyPostfix]
        private static void FollowerTask_Cook_Setup(Follower follower)
        {
            follower1 = follower;
            _dissentBubbleCoroutine = follower.StartCoroutine(DissentBubbleRoutine(follower));
        }*/

        //Enable cooking indicator (there is no progress bar for the chef in vanilla)
        /*[HarmonyPatch(typeof(FollowerTask_Cook), "OnDoingBegin")]
        [HarmonyPrefix]
        private static void FollowerTask_Cook_OnDoingBegin(Follower follower) //TODO change to worshipperbubble "food"?
        {
            follower1 = follower;
            if (_dissentBubbleCoroutine == null && follower1 != null)
                _dissentBubbleCoroutine = follower1.StartCoroutine(DissentBubbleRoutine(follower1));
        }*/

        //Enable devotion progress as cooking progress bar (there is no progress bar for the chef in vanilla)
        /*[HarmonyPatch(typeof(FollowerTask_Cook), "TaskTick")]
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

                if (Interaction_Kitchen.Kitchens.Count > 0) //turn off the fire
                {
                    Interaction_Kitchen.Kitchens[0].ShowCooking(false);
                }

                return false;

            }

            if (__instance.kitchenStructure.Data.CurrentCookingMeal == null)
            {
                __instance.kitchenStructure.Data.CurrentCookingMeal = __instance.kitchenStructure.Data.QueuedMeals[0];
                follower1.SetBodyAnimation("action", true);
                if (Interaction_Kitchen.Kitchens.Count > 0) //turn on the fire
                {
                    Plugin.Log.LogInfo("turning on fire");
                    Interaction_Kitchen.Kitchens[0].ShowCooking(true);
                }
            }

            else if (__instance.kitchenStructure.Data.CurrentCookingMeal.CookedTime >= __instance.kitchenStructure.Data.CurrentCookingMeal.CookingDuration)
            {
                if (follower1 != null)
                {
                    follower1.TimedAnimation("Reactions/react-laugh", 3.33f, (System.Action)(() => { 
                        __instance.ProgressTask();
                    }));
                    __instance.MealFinishedCooking();

                }
            }
            
            else
            {
                if (_dissentBubbleCoroutine == null && follower1 != null)
                {
                    _dissentBubbleCoroutine = follower1.StartCoroutine(DissentBubbleRoutine(follower1));
                }

                __instance.kitchenStructure.Data.CurrentCookingMeal.CookedTime += deltaGameTime * __instance._brain.Info.ProductivityMultiplier;

            }

            return false;
        }*/

        //Enable devotion progress as cooking progress bar (there is no progress bar for the chef in vanilla)
        /*[HarmonyPatch(typeof(FollowerTask_Cook), "Cleanup")]
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


        }*/

        //Add blocking task (temporary)
        /*[HarmonyPatch(typeof(FollowerTask), "BlockTaskChanges", MethodType.Getter)]
        [HarmonyPostfix]
        private static void FollowerTask_Cook_BlockTaskChanges(FollowerTask __instance, ref bool __result)
        {
            if (__instance.Type == FollowerTaskType.Cook)
            {
                __result = true;
            }
        }*/

        //cooking isnt a priority task so the chef will leave every time they finish one meal without this fix
        /*[HarmonyPatch(typeof(FollowerTask_Cook),nameof(FollowerTask_Cook.MealFinishedCooking))]
        [HarmonyPrefix]
        private static bool FollowerTask_Cook_MealFinishedCookingFix(FollowerTask_Cook __instance)
        {
            ++DataManager.Instance.MealsCooked;
            ObjectiveManager.CheckObjectives(Objectives.TYPES.COOK_MEALS);
            *//*Interaction_Kitchen kitchen = __instance.FindKitchen();
            if (kitchen != null)
            {
                kitchen.MealFinishedCooking();
            }*/
            /*else
            {*//*
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
            *//*}*//*
            if (__instance.kitchenStructure.Data.QueuedMeals.Count <= 0)
            {
                Plugin.Log.LogInfo("no more food to cook, waiting for new food");
                __instance.Complete();
            }
                

            return false;
        }*/

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
            
            if (Interaction_Kitchen.Kitchens[0].StructureInfo.Type == StructureBrain.TYPES.KITCHEN || Interaction_Kitchen.Kitchens[0].StructureInfo.Type == StructureBrain.TYPES.KITCHEN_II)
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
        /*[HarmonyPatch(typeof(FollowerTask_Cook), "UpdateDestination")]
        [HarmonyPostfix]
        private static void FollowerTask_Cook_UpdateDestination(FollowerTask_Cook __instance, ref Vector3 __result)
        {
            __result = __instance.kitchenStructure.Data.Position + new Vector3(0.0f, 2.521f);
        }*/

        //TODO: if there is no waiter, then the followers will go to the food
        [HarmonyPatch(typeof(FollowerBrain), "CheckEatTask")]
        [HarmonyPostfix]
        private static void FollowerBrain_CheckEatTask(ref FollowerTask __result)
        {
            if (!Plugin.waiterJob.Value) return;

            foreach (StructureBrain structurebrain in StructureManager.StructuresAtLocation(FollowerLocation.Base))
            {
                StructureBrain.TYPES structure = structurebrain.Data.Type;
                
                if (CustomStructureManager.CustomStructureList.ContainsKey(structure))
                {
                    if (CustomStructureManager.CustomStructureList[structure].InternalName == "Waiter_Desk_Structure")
                    {
                        __result = null;
                        return;
                    }
                }
            }

            return;
        }

        


        //this patch to enable movable kitchen
        [HarmonyPatch(typeof(PlacementRegion), "GetHoveredStructure")]
        [HarmonyPostfix]
        private static void PlacementRegion_GetHoveredStructure(PlacementRegion __instance, ref Structure __result)
        {
            PlacementTile tileAtWorldPosition = __instance.GetClosestTileAtWorldPosition(__instance.PlacementPosition);
            if (tileAtWorldPosition == null)
            {
                return;
            }

            PlacementRegion.TileGridTile tileGridTile = __instance.GetTileGridTile(tileAtWorldPosition.GridPosition.x, tileAtWorldPosition.GridPosition.y);

            Structure hoveredStructure = null;

            if (tileGridTile.ObjectOnTile == StructureBrain.TYPES.KITCHEN || tileGridTile.ObjectOnTile == StructureBrain.TYPES.KITCHEN_II)
            {
                foreach (Structure structure in Structure.Structures)
                {
                    if (structure.Type == StructureBrain.TYPES.COOKING_FIRE || structure.Type == StructureBrain.TYPES.KITCHEN || structure.Type == StructureBrain.TYPES.KITCHEN_II) //dont know why the kitchen upgrade doesnt make it into a cooking fire
                    {
                        if (structure.Brain != null && tileGridTile != null && structure.Brain.Data != null && structure.Brain.Data.ID == tileGridTile.ObjectID)
                        {
                            hoveredStructure = structure;
                            break;
                        }
                        if (hoveredStructure == null || structure != null && structure.Brain != null && structure.Brain.Data != null && tileGridTile != null && (double)Vector3.Distance(structure.Brain.Data.Position, tileGridTile.WorldPosition) < (double)Vector3.Distance(hoveredStructure.Brain.Data.Position, tileGridTile.WorldPosition))
                            hoveredStructure = structure;
                    }
                    
                }
                __result = hoveredStructure;
            }
        }

        //this patch to upgrade to kitchen 2
        [HarmonyPatch(typeof(Interaction_Kitchen), "Start")]
        [HarmonyPostfix]
        private static void Interaction_Kitchen_Start(Interaction_Kitchen __instance)
        {
            Plugin.Log.LogInfo("starting reassignment");
            if (__instance.StructureInfo.Type == StructureBrain.TYPES.KITCHEN)
            {
                Plugin.Log.LogInfo("Reassigned as kitchen");
                __instance.structure.Type = StructureBrain.TYPES.KITCHEN;
            }

            /*if (__instance.StructureInfo.Type == StructureBrain.TYPES.KITCHEN_II)
            {
                Plugin.Log.LogInfo("Reassigned as cooking fire");
                __instance.structure.Type = StructureBrain.TYPES.COOKING_FIRE;
            }*/
        }

        //this patch to skip cooking fire kitchen 2
        [HarmonyPatch(typeof(Interaction_Kitchen), "ShowCooking")]
        [HarmonyPrefix]
        private static bool Interaction_Kitchen_ShowCooking(Interaction_Kitchen __instance)
        {

            if (__instance.StructureInfo.Type == StructureBrain.TYPES.KITCHEN_II)
            {
                //Plugin.Log.LogInfo("skip fire changes");
                return false;
            }
            return true;
        }

        //this patch to skip some animations for kitchen 2
        /*[HarmonyPatch(typeof(Interaction_Kitchen), "CookAll")]
        [HarmonyPrefix]
        private static bool Interaction_Kitchen_CookAll(Interaction_Kitchen __instance)
        {

            if (__instance.StructureInfo.Type == StructureBrain.TYPES.KITCHEN_II)
            {
                GameManager.GetInstance().OnConversationNew();
                GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone);

                PlayerFarming.Instance.GoToAndStop(__instance.StructureInfo.Position + new Vector3(0.1f, 2.5f), __instance.transform.parent.gameObject, GoToCallback: ((System.Action)(() =>
                {
                    __instance._uiCookingMinigameOverlayController = MonoSingleton<UIManager>.Instance.CookingMinigameOverlayControllerTemplate.Instantiate<UICookingMinigameOverlayController>();
                    __instance._uiCookingMinigameOverlayController.Initialise(__instance.StructureInfo, __instance);
                    __instance._uiCookingMinigameOverlayController.OnCook += new System.Action(__instance.OnCook);
                    __instance._uiCookingMinigameOverlayController.OnUnderCook += new System.Action(__instance.OnUnderCook);
                    __instance._uiCookingMinigameOverlayController.OnBurn += new System.Action(__instance.OnBurn);
                    __instance.state.CURRENT_STATE = StateMachine.State.CustomAction0;
                })));
                return false;
            }
            return true;
        }*/

        public static Structures_FoodStorage GetAvailableFoodStorage(
            Vector3 fromPosition,
            FollowerLocation location)
        {
            List<Structures_FoodStorage> structuresOfType = StructureManager.GetAllStructuresOfType<Structures_FoodStorage>(location);
            List<Structures_FoodStorage> structuresFoodStorageList = new List<Structures_FoodStorage>();
            foreach (Structures_FoodStorage structuresFoodStorage in structuresOfType)
            {
                BoxCollider2D boxCollider2D = GameManager.GetInstance().GetComponent<BoxCollider2D>();
                if ((UnityEngine.Object)boxCollider2D == (UnityEngine.Object)null)
                {
                    boxCollider2D = GameManager.GetInstance().gameObject.AddComponent<BoxCollider2D>();
                    boxCollider2D.isTrigger = true;
                }
                boxCollider2D.size = Vector2.one * 12f;
                boxCollider2D.transform.position = structuresFoodStorage.Data.Position;
                boxCollider2D.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, -45f));
                if ((double)structuresFoodStorage.Data.Inventory.Count < (double)structuresFoodStorage.Capacity && boxCollider2D.OverlapPoint((Vector2)fromPosition))
                    structuresFoodStorageList.Add(structuresFoodStorage);
            }
            return structuresFoodStorageList.Count <= 0 ? (Structures_FoodStorage)null : structuresFoodStorageList[UnityEngine.Random.Range(0, structuresFoodStorageList.Count)];
        }

        //this patch to skip some animations for kitchen 2 on finish
        [HarmonyPatch(typeof(Interaction_Kitchen), "MealFinishedCooking")]
        [HarmonyPrefix]
        private static bool Interaction_Kitchen_MealFinishedCooking(Interaction_Kitchen __instance)
        {

            if (__instance.StructureInfo.Type == StructureBrain.TYPES.KITCHEN_II)
            {
                Structures_FoodStorage foodStorage = GetAvailableFoodStorage(__instance.StructureInfo.Position, __instance.StructureInfo.Location);
                InventoryItem.ITEM_TYPE mealType = __instance.structure.Structure_Info.QueuedMeals[0].MealType;
                ObjectiveManager.CompleteCustomObjective(Objectives.CustomQuestTypes.CookFirstMeal);
                DataManager.Instance.CookedFirstFood = true;


                if (mealType == InventoryItem.ITEM_TYPE.MEAL_POOP)
                {
                    ++DataManager.Instance.PoopMealsCreated;
                    /*if (DataManager.Instance.PoopMealsCreated == UnityEngine.Random.Range(5, 12) && !DataManager.GetFollowerSkinUnlocked("Poop"))
                        __instance.StartCoroutine(__instance.CreatePoopSkin());
                    else if (DataManager.Instance.PoopMealsCreated >= 12 && !DataManager.GetFollowerSkinUnlocked("Poop"))
                        __instance.StartCoroutine(__instance.CreatePoopSkin());*/
                }
                bool flag = false;

                if (foodStorage != null && __instance.StructureInfo != null)
                {
                    foreach (Interaction_FoodStorage foodStorage1 in Interaction_FoodStorage.FoodStorages)
                    {

                        Interaction_FoodStorage s = foodStorage1;
                        if (s != null && s.StructureInfo.ID == foodStorage.Data.ID && s.gameObject != null && __instance.transform != null)
                        {

                            ResourceCustomTarget.Create(s.gameObject, __instance.StructureInfo.Position, mealType, (System.Action)(() =>
                            {
                                AudioManager.Instance.PlayOneShot("event:/followers/pop_in", __instance.transform.position);
                                foodStorage.DepositItemUnstacked(mealType);
                                s.UpdateFoodDisplayed();
                                s.transform.DOPunchScale(Vector3.one * 0.25f, 0.25f, 2).SetEase<Tweener>(Ease.InOutBack);
                                foreach (Follower follower in Follower.Followers)
                                    follower.Brain.CheckChangeTask();
                            }));
                            flag = true;
                            break;
                        }
                    }
                }

                if (!flag)
                {
                    InventoryItem.Spawn(mealType, 1, __instance.StructureInfo.Position, UnityEngine.Random.Range(9, 11), (System.Action<PickUp>)(pickUp =>
                    {
                        Meal component = pickUp.GetComponent<Meal>();
                        component.CreateStructureLocation = __instance.StructureInfo.Location;
                        component.CreateStructureOnStop = true;
                    }));
                }
                else
                {
                    foreach (Follower follower in Follower.Followers)
                        follower.Brain.CheckChangeTask();
                }

                CookingData.CookedMeal(mealType);
                ++DataManager.Instance.MealsCooked;
                ObjectiveManager.CheckObjectives(Objectives.TYPES.COOK_MEALS);

                __instance.StructureInfo.QueuedMeals.RemoveAt(0);
                __instance.StructureInfo.CurrentCookingMeal = null;

                return false;
            }
            return true;
        }

        //extra slots
        [HarmonyPatch(typeof(RecipeQueue), nameof(RecipeQueue.RecipeLimit))]
        [HarmonyPostfix]
        public static void RecipeQueue_RecipeLimit(RecipeQueue __instance, ref int __result)
        {
            __result = 12 + (__instance._kitchenData.Type == StructureBrain.TYPES.KITCHEN_II ? 3 : 0);
        }

    }
}
