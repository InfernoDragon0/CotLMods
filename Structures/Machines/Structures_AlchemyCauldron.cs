using COTL_API.CustomStructures;
using COTL_API.Helpers;
using CotLMiniMods.Interactions;
using CotLTemplateMod;
using Lamb.UI.BuildMenu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CotLMiniMods.Structures
{
    internal class Structures_AlchemyCauldron : CustomStructure
    {
        public override string InternalName => "Structures_AlchemyCauldron";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/cauldron.png"));
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "Alchemy Cauldron";
        public override string GetLocalizedDescription() => "A Cauldron that allows you to fabricate necklaces using the power of Strange Materials.";

        public override List<StructuresData.ItemCost> Cost => new()
        {
            //new cost as you can now focus the necklace
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD,750),
            new(Plugin.StrangeMaterialItem, 25)
        };
        public List<InventoryItem.ITEM_TYPE> CookItems => new()
        {
            InventoryItem.ITEM_TYPE.Necklace_1,
            InventoryItem.ITEM_TYPE.Necklace_2,
            InventoryItem.ITEM_TYPE.Necklace_3,
            InventoryItem.ITEM_TYPE.Necklace_4,
            InventoryItem.ITEM_TYPE.Necklace_5,
            InventoryItem.ITEM_TYPE.Necklace_Dark,
            InventoryItem.ITEM_TYPE.Necklace_Demonic,
            InventoryItem.ITEM_TYPE.Necklace_Gold_Skull,
            InventoryItem.ITEM_TYPE.Necklace_Light,
            InventoryItem.ITEM_TYPE.Necklace_Loyalty,
            InventoryItem.ITEM_TYPE.Necklace_Missionary

        };

        public InventoryItem.ITEM_TYPE SelectedCookItem = InventoryItem.ITEM_TYPE.Necklace_1;

        public override FollowerCategory.Category Category => FollowerCategory.Category.Misc;
        public override Categories StructureCategories => Categories.ECONOMY;

        public override void OnAdded()
        {
            base.OnAdded();
        }

    }
}
