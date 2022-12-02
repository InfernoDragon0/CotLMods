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
    internal class Structures_EndlessPit : CustomStructure
    {
        public override string InternalName => "Structures_EndlessPit";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/cotlpc.png"));
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "Endless Pit of Gluttony";
        public override string GetLocalizedDescription() => "A machine that produces Strange Energy from dead followers.";

        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 20),
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
