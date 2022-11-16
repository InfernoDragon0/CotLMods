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

        public override string GetLocalizedName() => "Wishing Well";
        public override string GetLocalizedDescription() => "Give Coins. Get blessed.";

        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 50),
            new(InventoryItem.ITEM_TYPE.GOLD_REFINED, 5),
        };

        public override FollowerCategory.Category Category => FollowerCategory.Category.Misc;
        public override Categories Categories => Categories.ECONOMY;

        public override void OnAdded()
        {
            base.OnAdded();
        }

    }
}
