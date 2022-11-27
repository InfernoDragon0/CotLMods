using COTL_API.CustomStructures;
using COTL_API.Helpers;
using CotLTemplateMod;
using CotLMiniMods.CCommands.Tasks;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CotLMiniMods.Structures.Mines
{
    internal class SilkMineStructure : CustomStructure, ITaskProvider
    {
        public override string InternalName => "Silk_Mine_Structure";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/silkmines.png"));
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "Silk Mines";
        public override string GetLocalizedDescription() => "A place to harvest webs.";

        public int ResourceMax = 150;

        public bool CheckOverrideComplete() => true;

        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 55),
            new(InventoryItem.ITEM_TYPE.GOLD_REFINED, 3),
            new(InventoryItem.ITEM_TYPE.STONE, 15),
        };

        public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> sortedTasks)
        {
            if (activity != ScheduledActivity.Work || ReservedForTask || Data.Inventory.Count >= ResourceMax)
                return;

            FollowerTask_SilkMiner taskResourceStation = new FollowerTask_SilkMiner(Data.ID);
            sortedTasks.Add(taskResourceStation.Priorty, taskResourceStation);
        }

        public FollowerTask GetOverrideTask(FollowerBrain brain)
        {
            throw new NotImplementedException();
        }
    }
}
