using COTL_API.CustomTasks;
using CotLMiniMods.Structures.Productivity;
using CotLTemplateMod;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace CotLMiniMods.CCommands.Tasks
{
    internal class FollowerTask_DevoteNarinder : CustomTask
    {
        public override string InternalName => "FollowerTask_DevoteNarinder";
        private int _resourceStationID;
        private Structures_LuckyNarinder _resourceStation;
        public override int UsingStructureID => this._resourceStationID;
        public override bool BlockSocial => true;
        public override FollowerLocation Location => this._resourceStation.Data.Location;
        public override float Priorty => 20f;

        public FollowerTask_DevoteNarinder(int resourceStationID)
        {
            this._resourceStationID = resourceStationID;
            this._resourceStation = StructureManager.GetStructureByID<Structures_LuckyNarinder>(this._resourceStationID);
        }

        public FollowerTask_DevoteNarinder()
        {
            foreach (StructureBrain structureBrain in StructureManager.StructuresAtLocation(FollowerLocation.Base))
            {
                if (structureBrain is Structures_LuckyNarinder && !structureBrain.ReservedForTask)
                {
                    this._resourceStationID = structureBrain.Data.ID;
                    this._resourceStation = StructureManager.GetStructureByID<Structures_LuckyNarinder>(this._resourceStationID);
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
            return this._resourceStation.Data.Position + new Vector3(-1.5f, 0f);
        }
        
        public override void Setup(Follower follower)
        {
            base.Setup(follower);
            follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "pray"); //devotion anim
            Plugin.Log.LogInfo("the follower " + follower.Brain._directInfoAccess.Name + " is devoting to narinder");
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

            if (this._resourceStation.Data.Progress < 100)
                return;

            this._resourceStation.Data.Progress = 0.0f;


            Follower followerById = FollowerManager.FindFollowerByID(this._brain.Info.ID);

            if (this._brain.Info.ID == 666) //narinder does not gain faith but gain loyalty
            {
                followerById.TimedAnimation("devotion/devotion-collect-flame", 2.0f, (System.Action)(() => {
                    followerById.Brain.Stats.Adoration = followerById.Brain.Stats.MAX_ADORATION;
                    followerById.AdorationUI.BarController.SetBarSize(followerById.Brain.Stats.Adoration / followerById.Brain.Stats.MAX_ADORATION, false, true);
                    followerById.Brain.AddAdoration(followerById, FollowerBrain.AdorationActions.InspireLvl1, () =>
                    {
                        
                    });
                    
                    //to also drop give Strange Material at a chance on v1.1.1
                    this.End();
                }));
            }
            
            else
            {
                followerById.TimedAnimation("Reactions/react-laugh", 3.33f, (System.Action)(() => {
                    CultFaithManager.AddThought(Thought.ReactDecoration, this._brain.Info.ID, 1f); //SEASON 1 v1.1.0
                    //to also drop give Strange Material at a chance on v1.1.1
                }));
            }

            
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
            follower.SetHat(HatType.FaithEnforcer);
            follower.State.facingAngle = Utils.GetAngle(follower.transform.position, this._resourceStation.Data.Position);

        }

        public override void OnDoingBegin(Follower follower)
        {
            base.OnDoingBegin(follower);
            follower.SetHat(HatType.FaithEnforcer);
            follower.State.facingAngle = Utils.GetAngle(follower.transform.position, this._resourceStation.Data.Position);
        }
        private void OnNewPhaseStarted() => this.End();
    }
}
