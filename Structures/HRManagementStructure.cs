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
    internal class HRManagementStructure : CustomStructure
    {
        public override string InternalName => "HR_Management";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/cotlpc.png"));
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "Follower Manager";
        public override string GetLocalizedDescription() => "A modern device to manage your followers remotely.";

        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 30),
        };

        public override FollowerCategory.Category Category => FollowerCategory.Category.Misc;
        public override Categories Categories => Categories.ECONOMY;

        public override void OnAdded()
        {
            base.OnAdded();
        }

    }
}
