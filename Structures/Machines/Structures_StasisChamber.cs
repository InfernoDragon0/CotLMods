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
    internal class Structures_StasisChamber : CustomEnergyStructure
    {
        public override string InternalName => "Structures_StasisChamber";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/cauldron.png"));
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "Stasis Chamber";
        public override string GetLocalizedDescription() => "An exotic machine that uses Strange Material to freeze time.";

        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 150),
            new(Plugin.StrangeMaterialItem, 10)
        };

        public override FollowerCategory.Category Category => FollowerCategory.Category.Misc;
        public override Categories StructureCategories => Categories.ECONOMY;

        public override int EnergyRegenRate { get => 0; }
        public override bool CanAdd => true;
        public override bool CanRemove => false;
        public override bool WorksAtNight => true;
        public override bool WorksAtDay => true;

        public int energyPerPhase = 50;

        public bool timeFrozen = false;

        //write a custom logic that bypasses phase changes as pausing the simulation will not pass phases. use 50 energy per x time


        public override void OnAdded()
        {
            base.OnAdded();
            this.EnergyMax = 1000;
        }

    }
}
