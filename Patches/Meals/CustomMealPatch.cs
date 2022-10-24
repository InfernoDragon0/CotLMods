using CotLMiniMods.Items;
using CotLMiniMods.Meals;
using CotLTemplateMod;
using HarmonyLib;
using I2.Loc;
using Lamb.UI;
using Socket.Newtonsoft.Json;
using src.UI;
using src.UI.InfoCards;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.ProBuilder;

namespace CotLMiniMods.Patches.Meals
{
    [HarmonyPatch]
    internal class CustomMealPatch
    {
        // This patch adds food items to the menu
        [HarmonyPatch(typeof(CookingData), nameof(CookingData.GetAllMeals))]
        [HarmonyPostfix]
        public static void CookingData_GetAllMeals(CookingData __instance, ref InventoryItem.ITEM_TYPE[] __result)
        {
            if (CustomMealManager.FoodTypes.Count > 0)
            {
                var newList = new List<InventoryItem.ITEM_TYPE>();
                Plugin.Log.LogInfo(__result[0]);
                newList.AddRange(__result);

                foreach (InventoryItem.ITEM_TYPE type in CustomMealManager.FoodTypes.Keys)
                {
                    if (!newList.Contains(type))
                    {
                        newList.Add(type);
                    }
                }
                __result = newList.ToArray();

            }
        }

        // This patch unlocks the food types
        [HarmonyPatch(typeof(RecipesMenu), nameof(RecipesMenu.Configure))]
        [HarmonyPrefix]
        public static void RecipesMenu_Configure(RecipesMenu __instance)
        {
            Plugin.Log.LogInfo("Adding meals " + CustomMealManager.FoodTypes.Count);

            if (CustomMealManager.FoodTypes.Count > 0)
            {
                foreach (InventoryItem.ITEM_TYPE type in CustomMealManager.FoodTypes.Keys)
                {
                    if (!DataManager.Instance.RecipesDiscovered.Contains(type))
                    {
                        DataManager.Instance.RecipesDiscovered.Add(type);
                        Plugin.Log.LogInfo("Adding " + type);

                    }
                }
            }
        }

        // This patch adds the recipe
        [HarmonyPatch(typeof(CookingData), nameof(CookingData.GetRecipe))]
        [HarmonyPostfix]
        public static void CookingData_GetRecipe(CookingData __instance, ref List<List<InventoryItem>> __result, InventoryItem.ITEM_TYPE mealType)
        {
            if (CustomMealManager.FoodTypes.ContainsKey(mealType))
            {
                Plugin.Log.LogInfo("Getting meals recipe " + mealType);
                __result = CustomMealManager.FoodTypes[mealType].Recipe;
            }

        }

        // This patch adds the meal effects
        [HarmonyPatch(typeof(CookingData), nameof(CookingData.GetMealEffects))]
        [HarmonyPostfix]
        public static void CookingData_GetMealEffects(CookingData __instance, ref CookingData.MealEffect[] __result, InventoryItem.ITEM_TYPE mealType)
        {
            if (CustomMealManager.FoodTypes.ContainsKey(mealType))
            {
                Plugin.Log.LogInfo("Getting meals effects " + mealType);
                __result = CustomMealManager.FoodTypes[mealType].MealEffects;
            }

        }

        // This patch adds the stars
        [HarmonyPatch(typeof(CookingData), nameof(CookingData.GetSatationLevel))]
        [HarmonyPostfix]
        public static void CookingData_GetSatationLevel(CookingData __instance, ref int __result, InventoryItem.ITEM_TYPE meal)
        {
            if (CustomMealManager.FoodTypes.ContainsKey(meal))
            {
                Plugin.Log.LogInfo("Getting meals satiation " + meal);
                __result = CustomMealManager.FoodTypes[meal].SatiationLevel;
            }

        }

        // This patch adds the satiation amount
        [HarmonyPatch(typeof(CookingData), nameof(CookingData.GetSatationAmount))]
        [HarmonyPostfix]
        public static void CookingData_GetSatationAmount(CookingData __instance, ref int __result, InventoryItem.ITEM_TYPE meal)
        {
            if (CustomMealManager.FoodTypes.ContainsKey(meal))
            {
                Plugin.Log.LogInfo("Getting meals satiation amount " + meal);
                __result = CustomMealManager.FoodTypes[meal].SatiationAmount;
            }

        }

        // This patch adds the meal description
        [HarmonyPatch(typeof(RecipeInfoCard), nameof(RecipeInfoCard.Configure))]
        [HarmonyPostfix]
        public static void RecipeInfoCard_Configure(RecipeInfoCard __instance, InventoryItem.ITEM_TYPE config)
        {
            if (CustomMealManager.FoodTypes.ContainsKey(config))
            {
                Plugin.Log.LogInfo("Getting meals description " + config);
                __instance._itemHeader.text = CustomMealManager.FoodTypes[config].LocalizedName();
                __instance._itemDescription.text = CustomMealManager.FoodTypes[config].LocalizedDescription();
            }

        }

        // This patch adds the custom structure type
        [HarmonyPatch(typeof(StructuresData), nameof(StructuresData.GetInfoByType))]
        [HarmonyPostfix]
        public static void StructuresData_GetInfoByType(StructuresData __instance, StructureBrain.TYPES Type)
        {
            /*if (CustomMealManager.FoodTypes.ContainsKey(mealType))
            {
                Plugin.Log.LogInfo("Getting meals recipe " + mealType);
                __result = CustomMealManager.FoodTypes[mealType].Recipe;
            }*/

        }

        // This patch adds the custom structure type
        [HarmonyPatch(typeof(StructuresData), nameof(StructuresData.GetMealStructureType))]
        [HarmonyPostfix]
        public static void StructuresData_GetMealStructureType(StructuresData __instance, InventoryItem.ITEM_TYPE mealType, ref StructureBrain.TYPES __result)
        {
            if (CustomMealManager.FoodTypes.ContainsKey(mealType))
            {
                Plugin.Log.LogInfo("Getting meals structure custom " + mealType);
                __result = StructureBrain.TYPES.MEAL; //TODO
            }

        }


    }
}
