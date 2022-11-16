using COTL_API.CustomInventory;
using COTL_API.Helpers;
using CotLTemplateMod;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CotLMiniMods.Items
{
    internal class CustomFoodItem : CustomInventoryItem
    {
        public override InventoryItem.ITEM_TYPE ItemPickUpToImitate => InventoryItem.ITEM_TYPE.MEAL;
        public virtual List<List<InventoryItem>> Recipe => new List<List<InventoryItem>>()
        {
          new List<InventoryItem>()
          {
            new InventoryItem(InventoryItem.ITEM_TYPE.GRASS, 1)
          }
        };
        public virtual CookingData.MealEffect[] MealEffects => new CookingData.MealEffect[0];

        public virtual int SatiationLevel => 1;
        public virtual int SatiationAmount => 60;

        public override string InternalName => "Custom_Food_Item";
    }
}
