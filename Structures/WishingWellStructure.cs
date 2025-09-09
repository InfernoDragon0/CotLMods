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
    internal class WishingWellStructure : CustomStructure
    {
        public override string InternalName => "Wishing_Well";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/Icon_Pond.png"));
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "Wishing Well v2";
        public override string GetLocalizedDescription() => "Empower yourself with Tarot Cards and Relics of your choice at the start of a new run. Only one relic can be chosen.";

        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 500),
            new(InventoryItem.ITEM_TYPE.GOLD_REFINED, 5),
            new(Plugin.StrangeMaterialItem, 10),
        };

        public override FollowerCategory.Category Category => FollowerCategory.Category.Misc;
        public override Categories StructureCategories => Categories.ECONOMY;
    }
}
