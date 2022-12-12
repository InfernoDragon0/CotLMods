using COTL_API.Helpers;
using CotLMiniMods.CCommands.Tasks;
using Lamb.UI.BuildMenu;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CotLMiniMods.Structures
{
    internal class Structures_StrangeGenerator : CustomEnergyStructure
    {
        public override string InternalName => "Structures_StrangeGenerator";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/endlesspit.png"));
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "Strange Generator";
        public override string GetLocalizedDescription() => "A machine that converts Strange Material to Strange Energy on demand. Max 10000 Energy Stored.";

        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 200),
            new(Plugin.StrangeMaterialItem, 3)
        };

        public override FollowerCategory.Category Category => FollowerCategory.Category.Misc;
        public override Categories StructureCategories => Categories.ECONOMY;

        public override int EnergyRegenRate { get => 1000; set => this.energyPerFollower = value; }
        
        public override bool CanAdd => false;
        public override bool CanRemove => true;
        public override bool WorksAtNight => true;
        public override bool WorksAtDay => true;

        public int energyPerFollower = 1;

        public override void OnAdded()
        {
            base.OnAdded();
            this.EnergyMax = 10000;
        }

    }
}
