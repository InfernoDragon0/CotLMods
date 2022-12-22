using COTL_API.CustomStructures;
using COTL_API.Helpers;
using CotLMiniMods.CCommands.Tasks;

using CotLTemplateMod;
using CotLTemplateMod.CustomFollowerCommands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CotLMiniMods.Structures.Mines
{
    internal class Structures_GiftTree : CustomStructure, ITaskProvider
    {
        public override string InternalName => "Structures_GiftTree";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/crystalmines.png"));
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "Tree of Giving";
        public override string GetLocalizedDescription() => "Followers can work here to pack collectable gifts.";

        public int ResourceMax = 150;

        public bool CheckOverrideComplete() => true;

        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(Plugin.StrangeMaterialItem, 5),
            new(InventoryItem.ITEM_TYPE.LOG, 10),
        };

        public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> sortedTasks)
        {
            if (activity != ScheduledActivity.Work || ReservedForTask || Data.Inventory.Count >= ResourceMax)
                return;

            FollowerTask_GiftPacking taskResourceStation = new FollowerTask_GiftPacking(Data.ID);
            sortedTasks.Add(taskResourceStation.Priorty, taskResourceStation);
        }

        public FollowerTask GetOverrideTask(FollowerBrain brain)
        {
            throw new NotImplementedException();
        }
    }
}
