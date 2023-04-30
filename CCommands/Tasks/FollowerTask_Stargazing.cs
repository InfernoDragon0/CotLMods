using COTL_API.Tasks;
using CotLMiniMods.Structures.Productivity;
using CotLTemplateMod;
using UnityEngine;
using static UnityEngine.ParticleSystem.PlaybackState;

namespace CotLMiniMods.CCommands.Tasks
{
    internal class FollowerTask_Stargazing : CustomTask
    {
        //TODO: add an interaction to add strange materials into here
        public override string InternalName => "FollowerTask_Stargazing";
        private int _resourceStationID;
        private Structures_Telescope _resourceStation;
        public override int UsingStructureID => this._resourceStationID;
        public override bool BlockSocial => true;
        public override FollowerLocation Location => this._resourceStation.Data.Location;
        public override float Priorty => 20f;

        public FollowerTask_Stargazing(int resourceStationID)
        {
            this._resourceStationID = resourceStationID;
            this._resourceStation = StructureManager.GetStructureByID<Structures_Telescope>(this._resourceStationID);
        }

        public FollowerTask_Stargazing()
        {
            foreach (StructureBrain structureBrain in StructureManager.StructuresAtLocation(FollowerLocation.Base))
            {
                if (structureBrain is Structures_Telescope && !structureBrain.ReservedForTask)
                {
                    this._resourceStationID = structureBrain.Data.ID;
                    this._resourceStation = StructureManager.GetStructureByID<Structures_Telescope>(this._resourceStationID);
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
            follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "astrologer"); //devotion anim
            Plugin.Log.LogInfo("the follower " + follower.Brain._directInfoAccess.Name + " is stargazing");
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

            if (this._resourceStation.Data.Progress < 120)
                return;

            this._resourceStation.Data.Progress = 0.0f;


            Follower followerById = FollowerManager.FindFollowerByID(this._brain.Info.ID);

            
            followerById.TimedAnimation("Reactions/react-laugh", 3.33f, (System.Action)(() => {
                this._resourceStation.UsedForTheDay = true;
                Plugin.Log.LogInfo("Completed stargazing");
                
                if (TimeManager.IsNight)
                {
                    Plugin.Log.LogInfo("hidden interaction 2");
                    followerById.TimedAnimation("Hungry/get-hungry", 0.5f, (System.Action)(() =>
                    {
                        InventoryItem.Spawn(Plugin.StrangeMaterialItem, 5, followerById.transform.position);
                    }));
                }
                else
                {
                    if (Plugin.telescopeGivesQuest.Value)
                    {
                        Plugin.Log.LogInfo("give quest");
                        this.Brain.HardSwapToTask(new FollowerTask_GetAttention(Follower.ComplaintType.GiveQuest));
                    }
                    else
                    {
                        Plugin.Log.LogInfo("no give quest");
                        followerById.TimedAnimation("Hungry/get-hungry", 0.5f, (System.Action)(() =>
                        {
                            InventoryItem.Spawn(Plugin.StrangeMaterialItem, 1, followerById.transform.position);
                        }));
                    }
                }
                //this.End();
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
