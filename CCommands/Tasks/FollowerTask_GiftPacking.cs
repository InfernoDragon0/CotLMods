using COTL_API.CustomTasks;
using CotLMiniMods.Structures.Mines;
using CotLTemplateMod;
using Spine;
using UnityEngine;

namespace CotLMiniMods.CCommands.Tasks
{
    internal class FollowerTask_GiftPacking : CustomTask
    {
        public override string InternalName => "FollowerTask_GiftPacking";
        private int _resourceStationID;
        private Structures_GiftTree _resourceStation;
        public override int UsingStructureID => this._resourceStationID;
        public override bool BlockSocial => true;
        public override FollowerLocation Location => this._resourceStation.Data.Location;
        public override float Priorty => 20f;

        public FollowerTask_GiftPacking(int resourceStationID)
        {
            this._resourceStationID = resourceStationID;
            this._resourceStation = StructureManager.GetStructureByID<Structures_GiftTree>(this._resourceStationID);
        }

        public FollowerTask_GiftPacking()
        {
            foreach (StructureBrain structureBrain in StructureManager.StructuresAtLocation(FollowerLocation.Base))
            {
                if (structureBrain is Structures_GiftTree && !structureBrain.ReservedForTask)
                {
                    this._resourceStationID = structureBrain.Data.ID;
                    this._resourceStation = StructureManager.GetStructureByID<Structures_GiftTree>(this._resourceStationID);
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
            Plugin.Log.LogInfo("the follower is claimed on a gift tree");

            if (this._resourceStation == null)
                return;
            this._resourceStation.ReservedForTask = true;
        }

        public override void ReleaseReservations()
        {
            Plugin.Log.LogInfo("the follower is released on a gift tree");

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
            follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "action");
            Plugin.Log.LogInfo("the follower " + follower.Brain._directInfoAccess.Name + " is working on a gift tree");
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

            if (this._resourceStation.Data.Progress < 75)
                return;

            this._resourceStation.Data.Progress = 0.0f;

            if (this._resourceStation.Data.Inventory.Count >= this._resourceStation.ResourceMax)
                return;

            var ran = Random.Range(0, 2);

            var item = ran switch
            {
                0 => new InventoryItem(InventoryItem.ITEM_TYPE.GIFT_SMALL),
                1 => new InventoryItem(InventoryItem.ITEM_TYPE.GIFT_MEDIUM),
                _ => new InventoryItem(InventoryItem.ITEM_TYPE.GIFT_SMALL),
            };

            this._resourceStation.Data.Inventory.Add(item);

            Follower followerById = FollowerManager.FindFollowerByID(this._brain.Info.ID);
            followerById.TimedAnimation("Gifts/gift3", 0.75f, (System.Action)(() => {
                
            }));
        }

        public override void Cleanup(Follower follower)
        {
            base.Cleanup(follower);
            follower.SimpleAnimator.ResetAnimationsToDefaults();
            follower.SetHat(FollowerHatType.None);
        }


        public override void OnIdleBegin(Follower follower)
        {
            base.OnIdleBegin(follower);
            follower.State.facingAngle = Utils.GetAngle(follower.transform.position, this._resourceStation.Data.Position);


        }

        public override void OnDoingBegin(Follower follower)
        {
            base.OnDoingBegin(follower);
            follower.State.facingAngle = Utils.GetAngle(follower.transform.position, this._resourceStation.Data.Position);

        }
        private void OnNewPhaseStarted() => this.End();
    }
}
