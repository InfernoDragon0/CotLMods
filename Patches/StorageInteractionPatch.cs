using HarmonyLib;
using I2.Loc;
using MMTools;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CotLMiniMods.Patches
{
    [HarmonyPatch]
    internal class StorageInteractionPatch
    {

        private static bool Activating = false;
        private static float Delay = 0f;

        // This patch adds label to foodstorage
        [HarmonyPatch(typeof(Interaction), nameof(Interaction.Label), MethodType.Getter)]
        [HarmonyPostfix]
        public static void Interaction_Label(Interaction __instance, ref string __result)
        {
            Interaction_FoodStorage foodInstance;
            if ((foodInstance = __instance as Interaction_FoodStorage) != null)
            {
                __result = "Take out Food: " + foodInstance.CurrentCapacity + "/" + foodInstance.StructureBrain.Capacity;
            }
        }

        // This patch adds interaction to foodstorage
        [HarmonyPatch(typeof(Interaction), nameof(Interaction.OnInteract))]
        [HarmonyPostfix]
        public static void Interaction_OnInteract(Interaction __instance)
        {
            if (__instance is Interaction_FoodStorage)
            {
                if (!Activating)
                {
                    Activating = true;
                    //get inventory
                    var instanced = __instance as Interaction_FoodStorage;
                    if (instanced.StructureInfo.Inventory.Count <= 0)
                    {
                        Plugin.Log.LogInfo("No food in storage");
                    }
                    else
                    {
                        foreach (InventoryItem inventoryItem in instanced.StructureInfo.Inventory)
                        {
                            if (inventoryItem.UnreservedQuantity > 0)
                            {
                                Plugin.Log.LogInfo("Found stored food of type " + (InventoryItem.ITEM_TYPE)inventoryItem.type);
                                instanced.StructureBrain.TryClaimFoodReservation((InventoryItem.ITEM_TYPE)inventoryItem.type);
                                if (instanced.StructureBrain.TryEatReservedFood((InventoryItem.ITEM_TYPE)inventoryItem.type))
                                {
                                    Plugin.Log.LogInfo("removed stored food");
                                    StructureBrain.TYPES mealStructureType = StructuresData.GetMealStructureType((InventoryItem.ITEM_TYPE)inventoryItem.type);
                                    Vector3 position = PlayerFarming.Instance.transform.position + (Vector3)Random.insideUnitCircle * 2f;
                                    StructureManager.BuildStructure(FollowerLocation.Base, StructuresData.GetInfoByType(mealStructureType, 0), position, Vector2Int.one);
                                    Activating = false;
                                    break;
                                }
                            }
                        }
                        Activating = false;
                    }
                }


            }
        }

        // This patch adds interaction to foodstorage
        [HarmonyPatch(typeof(Interaction), nameof(Interaction.OnInteract))]
        [HarmonyPrefix]
        public static void Interaction_OnInteractPre(Interaction __instance)
        {
            if (__instance is Interaction_FoodStorage)
            {
                if (Activating)
                {
                    return;
                }
            }
        }

        // This patch allows to take out food form food storage
        /*[HarmonyPatch(typeof(Interaction_FoodStorage), nameof(Interaction_FoodStorage.Update))]
        [HarmonyPostfix]
        public static void Interaction_FoodStorage_Update(Interaction_FoodStorage __instance)
        {

            if (Activating && (__instance.StructureInfo.Inventory.Count <= 0 || InputManager.Gameplay.GetInteractButtonUp() || (double)Vector3.Distance(__instance._updatePos, __instance.transform.position) > (double)__instance.DistanceRadius))
                Activating = false;

            if ((double)(Delay -= Time.deltaTime) >= 0.0 || !Activating)
                return;

            Plugin.Log.LogInfo("now updating with activation");
            foreach (InventoryItem inventoryItem in __instance.StructureInfo.Inventory)
            {
                if (inventoryItem.UnreservedQuantity > 0)
                {
                    Plugin.Log.LogInfo("Found stored food of type " + (InventoryItem.ITEM_TYPE)inventoryItem.type);
                    __instance.StructureBrain.TryClaimFoodReservation((InventoryItem.ITEM_TYPE)inventoryItem.type);
                    if (__instance.StructureBrain.TryEatReservedFood((InventoryItem.ITEM_TYPE)inventoryItem.type))
                    {
                        Plugin.Log.LogInfo("removed stored food");
                        StructureBrain.TYPES mealStructureType = StructuresData.GetMealStructureType((InventoryItem.ITEM_TYPE)inventoryItem.type);
                        Vector3 position = __instance.StructureBrain.Data.Position + (Vector3)Random.insideUnitCircle * 2f;
                        StructureManager.BuildStructure(__instance.StructureBrain.Data.Location, StructuresData.GetInfoByType(mealStructureType, 0), position, Vector2Int.one);
                        break;
                    }

                }
            }
            Delay = 0.2f;
        }*/



    }
}
