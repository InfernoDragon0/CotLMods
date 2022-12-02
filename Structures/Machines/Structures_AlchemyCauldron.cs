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
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/cotlpc.png"));
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "Alchemy Cauldron";
        public override string GetLocalizedDescription() => "A machine that helps produce necklaces from Strange Material.";

        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 50),
            new(Plugin.StrangeMaterialItem, 10)
        };

        public override FollowerCategory.Category Category => FollowerCategory.Category.Misc;
        public override Categories StructureCategories => Categories.ECONOMY;

        public override void OnAdded()
        {
            base.OnAdded();
        }

    }
}
