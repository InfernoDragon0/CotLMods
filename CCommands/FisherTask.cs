using COTL_API.CustomTasks;
using CotLMiniMods.Structures.Mines;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CotLMiniMods.CustomFollowerCommands
{
    internal class FisherTask: CustomTask
    {
        public const int FISH_DURATION_GAME_MINUTES = 40;
        private int _fishingHutID;
        private FishingStructure _fishingHut;
        private float _progress;

        private int rngFish;

        public override FollowerTaskType Type => FollowerTaskType.Fisherman;

        public override FollowerLocation Location => this._fishingHut.Data.Location;

        public override int UsingStructureID => this._fishingHutID;

        public override float Priorty => 20f;

        public override string InternalName => "Fisher_Task";

        public override bool BlockTaskChanges => true;

        public override PriorityCategory GetPriorityCategory(
          FollowerRole FollowerRole,
          WorkerPriority WorkerPriority,
          FollowerBrain brain)
        {
            switch (FollowerRole)
            {
                case FollowerRole.Worshipper:
                case FollowerRole.Lumberjack:
                case FollowerRole.Farmer:
                case FollowerRole.Monk:
                    return PriorityCategory.Low;
                case FollowerRole.Worker:
                    return PriorityCategory.WorkPriority;
                default:
                    return PriorityCategory.Low;
            }
        }

        public FisherTask(int fishingHutID)
        {
            this._fishingHutID = fishingHutID;
            this._fishingHut = StructureManager.GetStructureByID<FishingStructure>(this._fishingHutID);
        }

        public override int GetSubTaskCode() => this._fishingHutID;

        public override void ClaimReservations() => StructureManager.GetStructureByID<FishingStructure>(this._fishingHutID).ReservedForTask = true;

        public override void ReleaseReservations() => StructureManager.GetStructureByID<FishingStructure>(this._fishingHutID).ReservedForTask = false;

        public override void OnStart()
        {
            this.SetState(FollowerTaskState.GoingTo);
        }

        public override void OnArrive() => this.SetState(FollowerTaskState.Idle);

        public override void TaskTick(float deltaGameTime)
        {
            if (this.State != FollowerTaskState.Idle && this.State != FollowerTaskState.Doing)
                return;

            this._progress += deltaGameTime * this._brain.Info.ProductivityMultiplier * 2;

            if (this._progress < 40.0)
                return;

            this._progress = 0.0f;

            FishingStructure structureById = StructureManager.GetStructureByID<FishingStructure>(this._fishingHutID);

            if (structureById.Data.Inventory.Count < 75)
            {
                //randomize the fish
                InventoryItem.ITEM_TYPE itemType = InventoryItem.ITEM_TYPE.FISH;
                Plugin.Log.LogInfo("Generating fish of " + rngFish);
                switch (rngFish)
                {
                    case 0:
                        itemType = InventoryItem.ITEM_TYPE.FISH_SMALL;
                        break;
                    case 1:
                        itemType = InventoryItem.ITEM_TYPE.FISH;
                        break;
                    case 2:
                        itemType = InventoryItem.ITEM_TYPE.FISH_SMALL;
                        break;
                    case 3:
                        itemType = InventoryItem.ITEM_TYPE.FISH;
                        break;
                    case 4:
                        itemType = InventoryItem.ITEM_TYPE.FISH;
                        break;
                    case 5:
                        itemType = InventoryItem.ITEM_TYPE.FISH_SQUID;
                        break;
                    case 6:
                        itemType = InventoryItem.ITEM_TYPE.FISH_BLOWFISH;
                        break;
                    case 7:
                        itemType = InventoryItem.ITEM_TYPE.FISH_CRAB;
                        break;
                    case 8:
                        itemType = InventoryItem.ITEM_TYPE.FISH_LOBSTER;
                        break;
                    case 9:
                        itemType = InventoryItem.ITEM_TYPE.FISH_OCTOPUS;
                        break;

                    default:
                        itemType = InventoryItem.ITEM_TYPE.FISH;
                        break;
                }
                structureById.Data.Inventory.Add(new InventoryItem(itemType));

            }

            this.SetState(FollowerTaskState.Doing);
        }

        public override Vector3 UpdateDestination(Follower follower)
        {
            
            return this._fishingHut.Data.Position + new Vector3(1.0f, 0.0f);
        }

        public override void Setup(Follower follower)
        {
            base.Setup(follower);
            follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "Fishing/fishing");
        }

        public override void OnIdleBegin(Follower follower)
        {
            base.OnIdleBegin(follower);
            follower.TimedAnimation("Fishing/fishing-start", 1.83333337f, (System.Action)(() => follower.State.CURRENT_STATE = StateMachine.State.Idle));
        }

        public override void OnDoingBegin(Follower follower)
        {
            rngFish = UnityEngine.Random.Range(0, 10);
            string str = new string[] { 
                "small", 
                "small", 
                "small", 
                "small", 
                "small", 
                "medium", 
                "medium", 
                "medium", 
                "big" ,
                "big" 
            }[rngFish];
            follower.TimedAnimation("Fishing/fishing-catch-" + str, 2.33333325f, (System.Action)(() =>
            {
                follower.State.CURRENT_STATE = StateMachine.State.Idle;
                this.SetState(FollowerTaskState.Idle);
                Plugin.Log.LogInfo("Catched fish of " + rngFish + " (" + str + ")");
            }));
        }


        public override void SimDoingBegin(SimFollower simFollower) => this.SetState(FollowerTaskState.Idle);
    }
}
