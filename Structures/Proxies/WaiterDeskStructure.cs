using COTL_API.CustomStructures;
using COTL_API.Helpers;
using CotLMiniMods.CCommands.Tasks;
using CotLTemplateMod;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CotLMiniMods.Structures.Proxies
{
    internal class WaiterDeskStructure : CustomStructure, ITaskProvider
    {
        public override string InternalName => "Waiter_Desk_Structure";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/waiterdesk.png"));
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "Waiter Desk";
        public override string GetLocalizedDescription() => "A Desk to allow followers to serve food to other followers.";

        public bool CheckOverrideComplete() => true;


        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10),
            new(InventoryItem.ITEM_TYPE.GRASS, 5),
            new(InventoryItem.ITEM_TYPE.LOG, 10),
        };



        public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> sortedTasks)
        {
            var count = 0;
            foreach (Structures_Meal structuresMeal in StructureManager.GetAllStructuresOfType<Structures_Meal>(FollowerLocation.Base))
            {
                if (!structuresMeal.Data.Rotten && !structuresMeal.Data.Burned && !structuresMeal.ReservedForTask)
                {
                    count++;
                }
            }

            foreach (Interaction_FollowerKitchen followerKitchen in Interaction_FollowerKitchen.FollowerKitchens)
            {

                foreach (InventoryItem inventoryItem in followerKitchen.foodStorage.StructureInfo.Inventory)
                {
                    if (inventoryItem.UnreservedQuantity > 0)
                    {
                        count++;
                    }
                }

            }

            if (activity != ScheduledActivity.Work || ReservedForTask || count == 0)
                return;

            WaiterTask taskResourceStation = new WaiterTask(Data.ID);
            sortedTasks.Add(taskResourceStation.Priorty, taskResourceStation);
        }

        public FollowerTask GetOverrideTask(FollowerBrain brain)
        {
            throw new NotImplementedException();
        }

    }
}
