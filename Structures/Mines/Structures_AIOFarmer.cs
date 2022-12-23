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
    internal class Structures_AIOFarmer : CustomStructure, ITaskProvider
    {
        public override string InternalName => "Structures_AIOFarmer";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/aio.png"));
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "Incubation Chamber";
        public override string GetLocalizedDescription() => "A machine powered by Strange Energy to incubate seeds.";

        public int ResourceMax = 1000;

        public bool CheckOverrideComplete() => true;

        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1000),
            new(Plugin.StrangeMaterialItem, 15),
        };

        public List<InventoryItem.ITEM_TYPE> QuarryItems => new()
        {
            InventoryItem.ITEM_TYPE.PUMPKIN,
            InventoryItem.ITEM_TYPE.BEETROOT,
            InventoryItem.ITEM_TYPE.CAULIFLOWER,
            InventoryItem.ITEM_TYPE.MUSHROOM_SMALL,
            InventoryItem.ITEM_TYPE.BERRY,
            InventoryItem.ITEM_TYPE.FLOWER_RED,
            InventoryItem.ITEM_TYPE.FLOWER_WHITE,
            InventoryItem.ITEM_TYPE.FLOWER_PURPLE,
            InventoryItem.ITEM_TYPE.GRASS,

        };

        public InventoryItem.ITEM_TYPE SelectedQuarryItem = InventoryItem.ITEM_TYPE.STONE;

        public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> sortedTasks)
        {
            if (activity != ScheduledActivity.Work || ReservedForTask || Data.Inventory.Count >= ResourceMax)
                return;

            FollowerTask_AIOFarmer taskResourceStation = new FollowerTask_AIOFarmer(Data.ID);
            sortedTasks.Add(taskResourceStation.Priorty, taskResourceStation);
        }

        public FollowerTask GetOverrideTask(FollowerBrain brain)
        {
            throw new NotImplementedException();
        }
    }
}
