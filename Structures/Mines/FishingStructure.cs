using COTL_API.CustomStructures;
using COTL_API.Helpers;
using CotLMiniMods.CCommands.Tasks;
using CotLMiniMods.CustomFollowerCommands;
using CotLMiniMods.Interactions;
using CotLTemplateMod;
using CotLTemplateMod.CustomFollowerCommands;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CotLMiniMods.Structures.Mines
{
    internal class FishingStructure : CustomStructure, ITaskProvider
    {
        public override string InternalName => "Fishing_Structure";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/Icon_FishingHut.png"));
        public override List<CustomStructureBuildingData> BuildingParts => [
            new CustomStructureBuildingData() {
                Sprite = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/fishing_hut_FRONT.png")),
                Offset = new Vector3(0, 0, -1.5975f),
                Scale = new Vector3(1, 1, 1),
                Rotation = new Vector3(300,0,0)
            },
            new CustomStructureBuildingData() {
                Sprite = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/fishing_hut_BACK.png")),
                Offset = new Vector3(0, -0.5f, -0.4f),
                Scale = new Vector3(1, 1, 1),
                Rotation = new Vector3(300,0,0)

            },
            new CustomStructureBuildingData() {
                Sprite = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/fishing_hut_PIER.png")),
                Offset = new Vector3(0.025f, -1.15f, -0.037f),
                Scale = new Vector3(1, 1, 1),
                Rotation = new Vector3(0,0,0)

            },
            new CustomStructureBuildingData() {
                Sprite = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/fishing_hut_POND.png")),
                Offset = new Vector3(0, -0.8f, -0.027f),
                Scale = new Vector3(1, 1, 1),
                Rotation = new Vector3(0,0,0)

            },
            new CustomStructureBuildingData() {
                Sprite = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/fishing_hut_ROD.png")),
                Offset = new Vector3(1.1f, -1.4f, -0.3f),
                Scale = new Vector3(1, 1, 1),
                Rotation = new Vector3(300,0,0)

            },
        ];
           
        
        public override int BuildDurationMinutes => 30;

        public override string GetLocalizedName() => "MiniMod's Fishing Hut";
        public override string GetLocalizedDescription() => "A place to farm lots of fishies of various types.";

        public int ResourceMax = 150;

        public bool CheckOverrideComplete() => true;

        public override Type Interaction => typeof(Interaction_CrystalMine);

        public override List<StructuresData.ItemCost> Cost => new()
        {
            new(InventoryItem.ITEM_TYPE.BLACK_GOLD, 6),
            new(InventoryItem.ITEM_TYPE.BONE, 5),
            new(InventoryItem.ITEM_TYPE.STONE, 5),
        };


        public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> sortedTasks)
        {
            if (activity != ScheduledActivity.Work || ReservedForTask || Data.Inventory.Count >= ResourceMax)
                return;

            FisherTask taskResourceStation = new(Data.ID);
            sortedTasks.Add(taskResourceStation.Priorty, taskResourceStation);
        }

        public FollowerTask GetOverrideTask(FollowerBrain brain)
        {
            throw new NotImplementedException();
        }

    }
}
