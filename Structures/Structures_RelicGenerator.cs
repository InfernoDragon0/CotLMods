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
    internal class Structures_RelicGenerator : CustomStructure
    {
        public override string InternalName => "Structures_RelicGenerator";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/Icon_Pond.png"));
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "Infernal Infuser";
        public override string GetLocalizedDescription() => "Trials of the Gods: Activate trials and augments to make your runs more challenging! Can you rise up to be the ultimate lamb?";

        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 666),
            new(Plugin.StrangeMaterialItem, 6),
        };

        public override FollowerCategory.Category Category => FollowerCategory.Category.Misc;
        public override Categories StructureCategories => Categories.ECONOMY;

    }
}
