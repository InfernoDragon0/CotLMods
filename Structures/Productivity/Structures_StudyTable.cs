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
    internal class Structures_StudyTable : CustomStructure, ITaskProvider
    {
        public override string InternalName => "Structures_StudyTable";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/study.png")); //TODO: change icon
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "Study Table";
        
        //SEASON 1: v1.1.0 only. v1.1.1 will change to provide strange material along with a quest.
        public override string GetLocalizedDescription() => "Allows your followers to research in Strange Materials. Invest Gold to increase generation. The follower will gain loyalty as well.";

        public bool CheckOverrideComplete() => true;

        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 15),
            new(InventoryItem.ITEM_TYPE.LOG_REFINED, 7),
            new(InventoryItem.ITEM_TYPE.GRASS, 10),
        };

        public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> sortedTasks)
        {
            if (activity != ScheduledActivity.Work || ReservedForTask)
                return;

            FollowerTask_StudyResearch taskResourceStation = new FollowerTask_StudyResearch(Data.ID); //TODO: change to study task
            sortedTasks.Add(taskResourceStation.Priorty, taskResourceStation);
        }

        public FollowerTask GetOverrideTask(FollowerBrain brain)
        {
            throw new NotImplementedException();
        }

    }
}
