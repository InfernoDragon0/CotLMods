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
    internal class Structures_Telescope : CustomStructure, ITaskProvider
    {
        public override string InternalName => "Structures_Telescope";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/telescope.png")); //TODO: change icon
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "Telescope";
        
        //SEASON 1: v1.1.0 only. v1.1.1 will change to provide strange material along with loyalty.
        public override string GetLocalizedDescription() => "Allows your followers to see the stars, and gain inspiration.";

        public bool CheckOverrideComplete() => true;

        public bool UsedForTheDay = false;

        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 15),
            new(InventoryItem.ITEM_TYPE.LOG_REFINED, 5),
            new(InventoryItem.ITEM_TYPE.STONE_REFINED, 5),
        };

        public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> sortedTasks)
        {
            if (activity != ScheduledActivity.Work || ReservedForTask || UsedForTheDay)
                return;

            FollowerTask_Stargazing taskResourceStation = new FollowerTask_Stargazing(Data.ID); //TODO: change to sightseeing task
            sortedTasks.Add(taskResourceStation.Priorty, taskResourceStation);
        }

        public FollowerTask GetOverrideTask(FollowerBrain brain)
        {
            throw new NotImplementedException();
        }

        public override void OnAdded() { 
            TimeManager.OnNewDayStarted += new System.Action(this.OnNewPhaseStarted); 
            TimeManager.OnNewPhaseStarted += new System.Action(this.CheckNight); 
        }

        public override void OnRemoved() { 
            TimeManager.OnNewDayStarted -= new System.Action(this.OnNewPhaseStarted);
            TimeManager.OnNewPhaseStarted -= new System.Action(this.CheckNight);
            
        }

        public override void OnNewPhaseStarted()
        {
            Plugin.Log.LogInfo("Reset use for the day for telescope");
            UsedForTheDay = false;
        }

        public void CheckNight()
        {
            if (TimeManager.IsNight)
            {
                Plugin.Log.LogInfo("Reset for the night");
                UsedForTheDay = false;
            }
        }
    }
}
