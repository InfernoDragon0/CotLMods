using COTL_API.CustomStructures;
using COTL_API.Helpers;
using CotLMiniMods.CCommands.Tasks;
using CotLMiniMods.Items;
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

        public override string GetLocalizedName() => "Sous Chef Desk";
        public override string GetLocalizedDescription() => "A Desk for the Sous Chef to create Signature Dishes.";

        public bool CheckOverrideComplete() => true;


        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 10),
            new(InventoryItem.ITEM_TYPE.GRASS, 5),
            new(InventoryItem.ITEM_TYPE.LOG, 10),
        };


        public List<InventoryItem.ITEM_TYPE> CookItems => new()
        {
            InventoryItem.ITEM_TYPE.MEAL_GRASS,
            InventoryItem.ITEM_TYPE.MEAL_POOP,
            InventoryItem.ITEM_TYPE.MEAL_BAD_FISH,
            InventoryItem.ITEM_TYPE.MEAL_BAD_MEAT,
            InventoryItem.ITEM_TYPE.MEAL_BAD_MIXED,
            InventoryItem.ITEM_TYPE.MEAL_BERRIES,
            InventoryItem.ITEM_TYPE.MEAL_MEAT,
            InventoryItem.ITEM_TYPE.MEAL_MUSHROOMS,
            InventoryItem.ITEM_TYPE.MEAL_MEDIUM_MIXED,
            InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG,

        };

        public InventoryItem.ITEM_TYPE SelectedCookItem = InventoryItem.ITEM_TYPE.MEAL_GRASS;


        public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> sortedTasks)
        {
            if (activity != ScheduledActivity.Work || ReservedForTask)
                return;
            if (Interaction_FollowerKitchen.FollowerKitchens.Count > 0) //change to follower kitchen
            {
                if (Interaction_FollowerKitchen.FollowerKitchens[0].StructureInfo.QueuedMeals.Count > 0)
                {
                    
                    FollowerTask_CookCustom cook = new FollowerTask_CookCustom(Data.ID);
                    sortedTasks.Add(cook.Priorty, cook);
                }
            }

            //the 2nd task is when followers are hungry
            if (FollowerManager.GetHungriestFollowerBrain().GetHungerScore() > 0) //need to check inventory if have strange material
            {
                Debug.Log("Someone is hungry at " + FollowerManager.GetHungriestFollowerBrain().GetHungerScore());
                FollowerTask_CookCustom cook = new FollowerTask_CookCustom(Data.ID);
                sortedTasks.Add(cook.Priorty, cook);
            }

        }

        public FollowerTask GetOverrideTask(FollowerBrain brain)
        {
            throw new NotImplementedException();
        }

    }
}
