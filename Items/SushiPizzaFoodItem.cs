using COTL_API.CustomInventory;
using COTL_API.Helpers;
using CotLTemplateMod;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CotLMiniMods.Items
{
    internal class SushiPizzaFoodItem : CustomFoodItem
    {
        public override string InternalName => "Sushi_Pizza";

        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/fish.png"));
        public override Sprite InventoryIcon { get; } = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/fish.png"));


        public override int SatiationLevel => 3;
        public override int SatiationAmount => 75;

        public override List<List<InventoryItem>> Recipe => new List<List<InventoryItem>>()
        {
            new List<InventoryItem>()
            {
                new InventoryItem(InventoryItem.ITEM_TYPE.FISH, 7)
            }
        };

        public override CookingData.MealEffect[] MealEffects => new CookingData.MealEffect[3]
        {
            new CookingData.MealEffect()
            {
            MealEffectType = CookingData.MealEffectType.DropLoot,
            Chance = 100
            },
            new CookingData.MealEffect()
            {
            MealEffectType = CookingData.MealEffectType.InstantlyPoop,
            Chance = 100
            },
            new CookingData.MealEffect()
            {
            MealEffectType = CookingData.MealEffectType.InstantlyDie,
            Chance = 100
            },
        };

    }
}
