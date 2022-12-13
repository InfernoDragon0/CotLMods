using COTL_API.Tasks;
using CotLMiniMods.Structures.Mines;
using CotLTemplateMod;
using Spine;
using UnityEngine;

namespace CotLMiniMods.CCommands.Tasks
{
    internal class FollowerTask_AIOMiner : CustomTask
    {
        public override string InternalName => "FollowerTask_AIOMiner";
        private int _resourceStationID;
        private Structures_AIOQuarry _resourceStation;
        public override int UsingStructureID => this._resourceStationID;
        public override bool BlockSocial => true;
        public override FollowerLocation Location => this._resourceStation.Data.Location;
        public override float Priorty => 20f;

        public FollowerTask_AIOMiner(int resourceStationID)
        {
            this._resourceStationID = resourceStationID;
            this._resourceStation = StructureManager.GetStructureByID<Structures_AIOQuarry>(this._resourceStationID);
        }

        public FollowerTask_AIOMiner()
        {
            foreach (StructureBrain structureBrain in StructureManager.StructuresAtLocation(FollowerLocation.Base))
            {
                if (structureBrain is Structures_AIOQuarry && !structureBrain.ReservedForTask)
                {
                    this._resourceStationID = structureBrain.Data.ID;
                    this._resourceStation = StructureManager.GetStructureByID<Structures_AIOQuarry>(this._resourceStationID);
                    return;
                }
                    
            }

            this.End();
        }

        public override PriorityCategory GetPriorityCategory(
          FollowerRole FollowerRole,
          WorkerPriority WorkerPriority,
          FollowerBrain brain)
        {
            switch (FollowerRole)
            {
                case FollowerRole.Worshipper:
                case FollowerRole.Farmer:
                case FollowerRole.Monk:
                    return PriorityCategory.Low;
                case FollowerRole.Lumberjack:
                case FollowerRole.Worker:
                    return PriorityCategory.WorkPriority;
                default:
                    return PriorityCategory.Low;
            }
        }

        public override int GetSubTaskCode() => this._resourceStationID;

        public override void ClaimReservations()
        {

            if (this._resourceStation == null)
                return;
            this._resourceStation.ReservedForTask = true;
        }

        public override void ReleaseReservations()
        {
            if (this._resourceStation == null)
                return;
            this._resourceStation.ReservedForTask = false;
        }

        public override Vector3 UpdateDestination(Follower follower)
        {
            return this._resourceStation.Data.Position + new Vector3(1.0f, 0.0f);
        }
        public override void Setup(Follower follower)
        {
            base.Setup(follower);
            follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "dig");
            Plugin.Log.LogInfo("the follower " + follower.Brain._directInfoAccess.Name + " is working on a AIO mine");
        }

        public override void OnStart()
        {
            TimeManager.OnNewPhaseStarted += new System.Action(this.OnNewPhaseStarted);
            this.SetState(FollowerTaskState.GoingTo);

        }
        public override void OnArrive()
        {
            this.SetState(FollowerTaskState.Idle);
        }

        public override void TaskTick(float deltaGameTime)
        {
            
            if (this.State != FollowerTaskState.Idle && this.State != FollowerTaskState.Doing)
                return;

            this._resourceStation.Data.Progress += deltaGameTime * this._brain.Info.ProductivityMultiplier;

            if (this._resourceStation.Data.Progress < 35)
                return;

            this._resourceStation.Data.Progress = 0.0f;

            if (this._resourceStation.Data.Inventory.Count >= this._resourceStation.ResourceMax)
                return;

            this._resourceStation.Data.Inventory.Add(new InventoryItem(this._resourceStation.SelectedQuarryItem));

            Follower followerById = FollowerManager.FindFollowerByID(this._brain.Info.ID);
            followerById.TimedAnimation("Reactions/react-laugh", 3.33f, () =>
            {

            });
        }

        public override void Cleanup(Follower follower)
        {
            base.Cleanup(follower);
            follower.SimpleAnimator.ResetAnimationsToDefaults();
            follower.SetHat(HatType.None);
        }


        public override void OnIdleBegin(Follower follower)
        {
            base.OnIdleBegin(follower);
            follower.SetHat(HatType.Miner);

        }

        public override void OnDoingBegin(Follower follower)
        {
            base.OnDoingBegin(follower);
            follower.SetHat(HatType.Miner);
        }
        private void OnNewPhaseStarted() => this.End();
    }
}
