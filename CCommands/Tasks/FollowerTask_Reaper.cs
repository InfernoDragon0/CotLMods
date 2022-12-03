using COTL_API.Tasks;
using CotLMiniMods.Structures;
using CotLMiniMods.Structures.Productivity;
using CotLMiniMods.Structures.Proxies;
using CotLTemplateMod;
using UnityEngine;

namespace CotLMiniMods.CCommands.Tasks
{
    internal class FollowerTask_Reaper : CustomTask
    {
        //TODO: add an interaction to add strange materials into here
        public override string InternalName => "FollowerTask_Reaper";
        private int _resourceStationID;
        private Structures_EndlessPit _resourceStation;
        private Structures_DeadWorshipper bodyToReap;
        private bool completeWrapping = false;
        public override int UsingStructureID => this._resourceStationID;
        public override bool BlockSocial => true;
        public override FollowerLocation Location => this._resourceStation.Data.Location;
        public override float Priorty => 20f;

        public FollowerTask_Reaper(int resourceStationID)
        {
            this._resourceStationID = resourceStationID;
            this._resourceStation = StructureManager.GetStructureByID<Structures_EndlessPit>(this._resourceStationID);

            foreach (StructureBrain structureBrain in StructureManager.StructuresAtLocation(FollowerLocation.Base))
            {
                if (structureBrain is Structures_DeadWorshipper && !structureBrain.ReservedForTask)
                {
                    bodyToReap = structureBrain as Structures_DeadWorshipper;
                    break;
                }
            }
        }

        public FollowerTask_Reaper()
        {
            foreach (StructureBrain structureBrain in StructureManager.StructuresAtLocation(FollowerLocation.Base))
            {
                if (structureBrain is Structures_DeadWorshipper && !structureBrain.ReservedForTask)
                {
                    bodyToReap = structureBrain as Structures_DeadWorshipper;
                    break;
                }
            }
            
            foreach (StructureBrain structureBrain in StructureManager.StructuresAtLocation(FollowerLocation.Base))
            {
                if (structureBrain is Structures_EndlessPit && !structureBrain.ReservedForTask)
                {
                    this._resourceStationID = structureBrain.Data.ID;
                    this._resourceStation = StructureManager.GetStructureByID<Structures_EndlessPit>(this._resourceStationID);
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
            return completeWrapping ? this._resourceStation.Data.Position : this.bodyToReap.Data.Position;
        }
        
        public override void Setup(Follower follower)
        {
            base.Setup(follower);
            follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "action");
            Plugin.Log.LogInfo("the follower " + follower.Brain._directInfoAccess.Name + " is burying");
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

            if (this._resourceStation.Data.Progress < 30)
                return;

            this._resourceStation.Data.Progress = 0.0f;
            
            Follower followerById = FollowerManager.FindFollowerByID(this._brain.Info.ID);

            DeadWorshipper deadWorshipper = FindCorpse();
            deadWorshipper.WrapBody();
            deadWorshipper.HideBody();
            followerById.TimedAnimation("Reactions/react-laugh", 3.33f, (System.Action)(() => {
                if (completeWrapping)
                {
                    /*DeadWorshipper deadWorshipper = FindCorpse();*/
                    PlacementRegion.TileGridTile tileAtWorldPosition = PlacementRegion.Instance.GetClosestTileGridTileAtWorldPosition(deadWorshipper.transform.position);
                    if (tileAtWorldPosition != null)
                        deadWorshipper.Structure.Brain.RemoveFromGrid(tileAtWorldPosition.Position);
                    StructureManager.RemoveStructure(deadWorshipper.Structure.Brain);
                    this._resourceStation.Data.Inventory.Add(new InventoryItem(InventoryItem.ITEM_TYPE.BONE));
                    this.End();
                }
                else
                {
                    
                    followerById.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "run-corpse");
                    followerById.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "dig");
                    if (_currentDestination.HasValue) //move to pit
                    {
                        ClearDestination();
                    }
                    SetState(FollowerTaskState.GoingTo);
                }
                
            }));

            
        }

        public override void Cleanup(Follower follower)
        {
            base.Cleanup(follower);
            DeadWorshipper corpse = this.FindCorpse();
            if (corpse != null)
            {
                corpse.StructureInfo.Position = follower.transform.position;
                corpse.transform.position = follower.transform.position;
                corpse.ShowBody();
            }
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

        private DeadWorshipper FindCorpse()
        {
            DeadWorshipper corpse = (DeadWorshipper)null;
            foreach (DeadWorshipper deadWorshipper in DeadWorshipper.DeadWorshippers)
            {
                if (deadWorshipper.StructureInfo.ID == this.bodyToReap.Data.ID)
                {
                    corpse = deadWorshipper;
                    break;
                }
            }
            return corpse;
        }
    }
}
