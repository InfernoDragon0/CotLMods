using COTL_API.CustomStructures;
using COTL_API.Helpers;
using CotLMiniMods.CCommands.Tasks;
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
    internal class Structures_EndlessPit : CustomStructure, ITaskProvider
    {
        public override string InternalName => "Structures_EndlessPit";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/endlesspit.png"));
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

        public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> sortedTasks)
        {
            var count = 0;
            foreach (Structures_DeadWorshipper deadWorshipper in StructureManager.GetAllStructuresOfType<Structures_DeadWorshipper>(FollowerLocation.Base))
            {
                if (!deadWorshipper.Data.Rotten && !deadWorshipper.ReservedForTask)
                {
                    count++;
                }
            }

            if (activity != ScheduledActivity.Work || ReservedForTask || count == 0 || !Plugin.reaperJob.Value)
                return;

            FollowerTask_Reaper taskResourceStation = new FollowerTask_Reaper(Data.ID);
            sortedTasks.Add(taskResourceStation.Priorty, taskResourceStation);
        }

        public FollowerTask GetOverrideTask(FollowerBrain brain)
        {
            throw new NotImplementedException();
        }

        public bool CheckOverrideComplete()
        {
            return true;
        }
    }
}
