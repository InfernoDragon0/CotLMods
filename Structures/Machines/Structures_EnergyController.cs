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
    internal class Structures_EnergyController : CustomEnergyStructure
    {
        public override string InternalName => "Structures_EnergyController";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/cauldron.png"));
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "Energy Controller";
        public override string GetLocalizedDescription() => "A device that collects and transfers Strange Energy. Stores up to 10000 Strange Energy";

        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 150),
            new(Plugin.StrangeMaterialItem, 10)
        };

        public override FollowerCategory.Category Category => FollowerCategory.Category.Misc;
        public override Categories StructureCategories => Categories.ECONOMY;

        public override int EnergyRegenRate { get => 0; }
        public override bool CanAdd => true;
        public override bool CanRemove => true;
        public override bool WorksAtNight => true;
        public override bool WorksAtDay => true;

        public int energyPerPhase = 0;

        public bool timeFrozen = false;

        public override void OnAdded()
        {
            base.OnAdded();
            this.EnergyMax = 10000;
        }

    }
}
