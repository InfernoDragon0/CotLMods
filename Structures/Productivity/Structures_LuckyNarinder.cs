using COTL_API.CustomStructures;
using COTL_API.Helpers;
using CotLMiniMods.CCommands.Tasks;
using CotLTemplateMod;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CotLMiniMods.Structures.Productivity
{
    internal class Structures_LuckyNarinder : CustomStructure, ITaskProvider
    {
        public override string InternalName => "Structures_LuckyNarinder";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/luckynarinder.png")); //TODO: change icon
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "Lucky Narinder Statue";
        
        //SEASON 1: v1.1.0 only. v1.1.1 will change to provide strange material additionaly
        public override string GetLocalizedDescription() => "Follower can admire this statue and pray to it. Provides Faith.";

        public bool CheckOverrideComplete() => true;

        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 15),
            new(InventoryItem.ITEM_TYPE.LOG_REFINED, 5),
            new(InventoryItem.ITEM_TYPE.STONE_REFINED, 5),
        };

        public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> sortedTasks)
        {
            if (activity != ScheduledActivity.Work || ReservedForTask)
                return;

            FollowerTask_DevoteNarinder taskResourceStation = new FollowerTask_DevoteNarinder(Data.ID);
            sortedTasks.Add(taskResourceStation.Priorty, taskResourceStation);
        }

        public FollowerTask GetOverrideTask(FollowerBrain brain)
        {
            throw new NotImplementedException();
        }

    }
}
