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
    internal class Structures_AIOQuarry : CustomStructure, ITaskProvider
    {
        public override string InternalName => "Structures_AIOQuarry";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/crystalmines.png"));
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "AIO Quarry";
        public override string GetLocalizedDescription() => "A machine powered by Strange Energy to generate your preferred output.";

        public int ResourceMax = 1000;

        public bool CheckOverrideComplete() => true;

        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1500),
            new(Plugin.StrangeMaterialItem, 20),
        };

        public List<InventoryItem> QuarryItems => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD),
            new(InventoryItem.ITEM_TYPE.STONE),
            new(InventoryItem.ITEM_TYPE.STONE_REFINED),
            new(InventoryItem.ITEM_TYPE.LOG),
            new(InventoryItem.ITEM_TYPE.LOG_REFINED),
            new(InventoryItem.ITEM_TYPE.SHELL),
            new(InventoryItem.ITEM_TYPE.POOP),
            new(InventoryItem.ITEM_TYPE.GRASS),
            new(InventoryItem.ITEM_TYPE.MEAT),
            new(InventoryItem.ITEM_TYPE.SEED_BEETROOT),
            new(InventoryItem.ITEM_TYPE.SEED_MUSHROOM),
            new(InventoryItem.ITEM_TYPE.SEED_CAULIFLOWER),
            new(InventoryItem.ITEM_TYPE.WHEAT),
            new(InventoryItem.ITEM_TYPE.SEED_PUMPKIN),
            new(InventoryItem.ITEM_TYPE.SEED_FLOWER_WHITE),

        };

        public InventoryItem.ITEM_TYPE SelectedQuarryItem = InventoryItem.ITEM_TYPE.STONE;

        public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> sortedTasks)
        {
            if (activity != ScheduledActivity.Work || ReservedForTask || Data.Inventory.Count >= ResourceMax)
                return;

            FollowerTask_AIOMiner taskResourceStation = new FollowerTask_AIOMiner(Data.ID);
            sortedTasks.Add(taskResourceStation.Priorty, taskResourceStation);
        }

        public FollowerTask GetOverrideTask(FollowerBrain brain)
        {
            throw new NotImplementedException();
        }
    }
}
