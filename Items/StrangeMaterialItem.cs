using COTL_API.CustomInventory;
using COTL_API.Helpers;
using CotLTemplateMod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CotLMiniMods.Items
{
    internal class StrangeMaterialItem : CustomInventoryItem
    {
        public override string InternalName => "Strange_Material";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/strange.png"));
        public override Sprite InventoryIcon { get; } = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/strange.png"));

        public override bool AddItemToDungeonChests => true;
        public override bool AddItemToOfferingShrine => true;
        public override int DungeonChestSpawnChance => 10;
        public override int DungeonChestMinAmount => 1;
        public override int DungeonChestMaxAmount => 1;
    }
}
