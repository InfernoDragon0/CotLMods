using COTL_API.CustomStructures;
using COTL_API.Helpers;
using CotLMiniMods.CCommands.Tasks;
using CotLTemplateMod;
using CotLTemplateMod.CustomFollowerCommands;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CotLMiniMods.Structures.Proxies
{
    internal class ChefDeskStructure : CustomStructure, ITaskProvider
    {
        public override string InternalName => "Chef_Desk_Structure";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/chefdesk.png"));
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "Chef Desk";
        public override string GetLocalizedDescription() => "A Desk to allow followers to cook food to other followers.";

        public bool CheckOverrideComplete() => true;


        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10),
            new(InventoryItem.ITEM_TYPE.GRASS, 5),
            new(InventoryItem.ITEM_TYPE.LOG, 10),
        };


        public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> sortedTasks)
        {
            if (activity != ScheduledActivity.Work || ReservedForTask)
                return;
            if (Interaction_Kitchen.Kitchens.Count > 0)
            {
                if (Interaction_Kitchen.Kitchens[0].StructureInfo.QueuedMeals.Count > 0)
                {
                    FollowerTask_CookCustom cook = new FollowerTask_CookCustom(Interaction_Kitchen.Kitchens[0].structure.Structure_Info.ID, Data.ID);
                    sortedTasks.Add(cook.Priorty, cook);
                }
            }

        }

        public FollowerTask GetOverrideTask(FollowerBrain brain)
        {
            throw new NotImplementedException();
        }

    }
}
