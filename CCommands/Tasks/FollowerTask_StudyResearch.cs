using COTL_API.Tasks;
using CotLMiniMods.Structures.Productivity;
using CotLTemplateMod;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace CotLMiniMods.CCommands.Tasks
{
    internal class FollowerTask_StudyResearch : CustomTask
    {
        //TODO: add an interaction to add strange materials into here
        public override string InternalName => "FollowerTask_StudyResearch";
        private int _resourceStationID;
        private Structures_StudyTable _resourceStation;
        public override int UsingStructureID => this._resourceStationID;
        public override bool BlockSocial => true;
        public override FollowerLocation Location => this._resourceStation.Data.Location;
        public override float Priorty => 20f;

        public FollowerTask_StudyResearch(int resourceStationID)
        {
            this._resourceStationID = resourceStationID;
            this._resourceStation = StructureManager.GetStructureByID<Structures_StudyTable>(this._resourceStationID);
        }

        public FollowerTask_StudyResearch()
        {
            foreach (StructureBrain structureBrain in StructureManager.StructuresAtLocation(FollowerLocation.Base))
            {
                if (structureBrain is Structures_StudyTable && !structureBrain.ReservedForTask)
                {
                    this._resourceStationID = structureBrain.Data.ID;
                    this._resourceStation = StructureManager.GetStructureByID<Structures_StudyTable>(this._resourceStationID);
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
                case FollowerRole.Lumberjack:
                    return PriorityCategory.Low;
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
            return this._resourceStation.Data.Position; //TODO: +- based on proximity to the statue
        }
        
        public override void Setup(Follower follower)
        {
            base.Setup(follower);
            follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "studying"); //devotion anim
            Plugin.Log.LogInfo("the follower " + follower.Brain._directInfoAccess.Name + " is studying");
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

            if (this._resourceStation.Data.Progress < 180)
                return;

            this._resourceStation.Data.Progress = 0.0f;


            Follower followerById = FollowerManager.FindFollowerByID(this._brain.Info.ID);

            
            followerById.TimedAnimation("Reactions/react-laugh", 3.33f, (System.Action)(() => {
                followerById.Brain.Stats.Adoration += 10;

                followerById.AdorationUI.BarController.SetBarSize(followerById.Brain.Stats.Adoration / followerById.Brain.Stats.MAX_ADORATION, false, true);
                followerById.StartCoroutine(followerById.Brain.AddAdorationIE(followerById, FollowerBrain.AdorationActions.InspireLvl1, null)); //SEASON 1 v1.1.0
                
                //to also convert Strange Materials to [REDACTED] v1.1.1
                if (this._resourceStation.Data.Inventory.Count > 0)
                {
                    InventoryItem.Spawn((InventoryItem.ITEM_TYPE)this._resourceStation.Data.Inventory[0].type, 1, followerById.transform.position);
                    this._resourceStation.Data.Inventory.RemoveAt(0);
                }
            }));

            
        }

        public override void Cleanup(Follower follower)
        {
            base.Cleanup(follower);
            follower.SimpleAnimator.ResetAnimationsToDefaults();
        }


        public override void OnIdleBegin(Follower follower)
        {
            base.OnIdleBegin(follower);

        }

        public override void OnDoingBegin(Follower follower)
        {
            base.OnDoingBegin(follower);
        }
        private void OnNewPhaseStarted() => this.End();
    }
}
