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
        public override string GetLocalizedDescription() => "A pit that emits Strange Energy from dead followers.";

        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 20),
            new(Plugin.StrangeMaterialItem, 10)
        };

        public override FollowerCategory.Category Category => FollowerCategory.Category.Misc;
        public override Categories StructureCategories => Categories.ECONOMY;

        public int currentEnergy = 0;
        public int maxEnergy = 1000;
        public int energyPerFollower = 1;

        public override void OnAdded()
        {
            TimeManager.OnNewPhaseStarted += new System.Action(this.OnNewPhaseStarted);
        }

        public override void OnRemoved()
        {
            TimeManager.OnNewPhaseStarted -= new System.Action(this.OnNewPhaseStarted);

        }

        public void OnNewPhaseStarted()
        {
            Plugin.Log.LogInfo("Add more energy");
            if (this.currentEnergy + this.Data.Inventory.Count > this.maxEnergy)
            {
                this.currentEnergy = this.maxEnergy;
            }
            else
            {
                this.currentEnergy += this.Data.Inventory.Count;
            }
        }

    }
}
