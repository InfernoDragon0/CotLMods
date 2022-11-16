using COTL_API.CustomStructures;
using COTL_API.Helpers;
using CotLMiniMods.CCommands.Tasks;

using CotLTemplateMod;
using CotLTemplateMod.CustomFollowerCommands;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CotLMiniMods.Structures
{
    internal class BoneMineStructure : CustomStructure, ITaskProvider
    {
        public override string InternalName => "Bone_Mine_Structure";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/bonemines.png"));
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "Bone Mines";
        public override string GetLocalizedDescription() => "A place to mine bones.";

        public int ResourceMax = 150;

        public bool CheckOverrideComplete() => true;

        

        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 6),
            new(InventoryItem.ITEM_TYPE.BONE, 5),
            new(InventoryItem.ITEM_TYPE.STONE, 5),
        };


        public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> sortedTasks)
        {
            if (activity != ScheduledActivity.Work || this.ReservedForTask || this.Data.Inventory.Count >= this.ResourceMax)
                return;

            FollowerTask_BoneMiner taskResourceStation = new FollowerTask_BoneMiner(this.Data.ID);
            sortedTasks.Add(taskResourceStation.Priorty, taskResourceStation);
        }

        public FollowerTask GetOverrideTask(FollowerBrain brain)
        {
            throw new NotImplementedException();
        }

    }
}
