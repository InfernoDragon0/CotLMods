using COTL_API.CustomStructures;
using COTL_API.Helpers;
using CotLMiniMods.Interactions;
using CotLMiniMods.Structures.Mines;
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
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1000),
            new(Plugin.StrangeMaterialItem, 25)
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
            TimeManager.OnNewPhaseStarted += new System.Action(this.OnNewPhaseStarted);
        }

        public override void OnRemoved()
        {
            TimeManager.OnNewPhaseStarted -= new System.Action(this.OnNewPhaseStarted);

        }

        public void OnNewPhaseStarted()
        {
            //two separate loops, one for collection and one for distribution
            Plugin.Log.LogInfo("Collecting energy..");
            foreach (StructureBrain structureBrain in StructureManager.StructuresAtLocation(FollowerLocation.Base))
            {
                if (structureBrain is CustomEnergyStructure energyStructure && energyStructure.CanRemove)
                {
                    Plugin.Log.LogInfo("Found energy structure: " + energyStructure.InternalName + " with " + energyStructure.EnergyCurrent + " energy");
                    int energyToAdd;
                    if (energyStructure.EnergyCurrent + this.EnergyCurrent >= this.EnergyMax)
                    {
                        energyToAdd = this.EnergyMax - this.EnergyCurrent;
                    }
                    else
                    {
                        energyToAdd = energyStructure.EnergyCurrent;
                    }
                    Plugin.Log.LogInfo("Before adding, this structure has " + this.EnergyCurrent + " energy");
                    this.AddEnergy(energyToAdd);
                    energyStructure.RemoveEnergy(energyToAdd);
                    Plugin.Log.LogInfo("Now this structure has " + this.EnergyCurrent + " energy");
                    return;
                }
            }
            Plugin.Log.LogInfo("Distributing Energy..");
            foreach (StructureBrain structureBrain in StructureManager.StructuresAtLocation(FollowerLocation.Base))
            {
                if (structureBrain is CustomEnergyStructure energyStructure && energyStructure.CanAdd)
                {
                    Plugin.Log.LogInfo("Found to add structure: " + energyStructure.InternalName + " with " + energyStructure.EnergyCurrent + " energy");
                    int energyToAdd;
                    if (energyStructure.EnergyCurrent + this.EnergyCurrent >= this.EnergyMax)
                    {
                        energyToAdd = this.EnergyMax - this.EnergyCurrent;
                    }
                    else
                    {
                        energyToAdd = energyStructure.EnergyCurrent;
                    }
                    Plugin.Log.LogInfo("Before removing, that structure has " + energyStructure.EnergyCurrent + " energy");
                    this.RemoveEnergy(energyToAdd);
                    energyStructure.AddEnergy(energyToAdd);
                    Plugin.Log.LogInfo("Now that structure has " + energyStructure.EnergyCurrent + " energy");
                    Plugin.Log.LogInfo("Now this structure has " + this.EnergyCurrent + " energy");
                    return;
                }
            }
        }
    }
}
