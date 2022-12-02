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
        public override string LocalizedName() { return "Strange Energy"; }
        public override string LocalizedDescription() { return "Unstable Energy harvested from Strange Material. This is a Proxy Item and not meant to be collected."; }

        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/fish.png"));
        public override Sprite InventoryIcon { get; } = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/fish.png"));

        public override bool AddItemToDungeonChests => false;
        public override bool AddItemToOfferingShrine => false;
        public override int DungeonChestSpawnChance => 0;
        public override int DungeonChestMinAmount => 0;
        public override int DungeonChestMaxAmount => 0;
    }
}
