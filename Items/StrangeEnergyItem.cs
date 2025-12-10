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
    internal class StrangeEnergyItem : CustomInventoryItem
    {
        public override string InternalName => "Strange_Energy";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/fish.png"));
        public override Sprite InventoryIcon { get; } = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/fish.png"));

        public override bool AddItemToDungeonChests => false;
        public override bool AddItemToOfferingShrine => false;
        public override int DungeonChestSpawnChance => 0;
        public override int DungeonChestMinAmount => 0;
        public override int DungeonChestMaxAmount => 0;
    }
}
