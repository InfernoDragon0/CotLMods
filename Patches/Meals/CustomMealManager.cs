using COTL_API.CustomInventory;
using CotLMiniMods.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace CotLMiniMods.Meals
{
    internal class CustomMealManager
    {
        internal static SushiPizzaFoodItem sushiPizzaFoodItem;
        internal static Dictionary<InventoryItem.ITEM_TYPE, CustomFoodItem> FoodTypes = new();

        public static void AddAll()
        {
            sushiPizzaFoodItem = new SushiPizzaFoodItem();
            FoodTypes.Add(CustomItemManager.Add(sushiPizzaFoodItem), sushiPizzaFoodItem);

        }


    }
}
