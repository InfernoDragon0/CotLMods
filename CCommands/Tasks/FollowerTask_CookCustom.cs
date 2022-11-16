using COTL_API.Tasks;
using CotLMiniMods.Structures;
using CotLTemplateMod;
using Spine;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine;

namespace CotLMiniMods.CCommands.Tasks
{
    internal class FollowerTask_CookCustom : CustomTask
    {
        public override string InternalName => "FollowerTask_CookCustom";
        public override int UsingStructureID => this.kitchenStationID;
        public override bool BlockSocial => true;
        public override bool BlockTaskChanges => true;
        public override FollowerLocation Location => this.kitchenStructure.Data.Location;
        public override float Priorty => 20f;

        private Structures_Kitchen kitchenStructure;
        private int kitchenStationID;

        private ChefDeskStructure chefDeskStructure;
        private int chefDeskID;
        public override Vector3 UpdateDestination(Follower follower) => this.kitchenStructure.Data.Position + new Vector3(0.0f, 2.521f);

        public override FollowerTaskType Type => FollowerTaskType.Cook;

        public Follower followerChef;
        private static Coroutine _dissentBubbleCoroutine;

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
                case FollowerRole.Chef:
                    return PriorityCategory.WorkPriority;
                default:
                    return PriorityCategory.Low;
            }
        }

        public FollowerTask_CookCustom(int kitchenStationID, int desk)
        {
            this.kitchenStationID = kitchenStationID;
            this.kitchenStructure = StructureManager.GetStructureByID<Structures_Kitchen>(kitchenStationID);

            this.chefDeskID = desk;
            this.chefDeskStructure = StructureManager.GetStructureByID<ChefDeskStructure>(desk);
        }

        public FollowerTask_CookCustom(int kitchenStationID)
        {
            this.kitchenStationID = kitchenStationID;
            this.kitchenStructure = StructureManager.GetStructureByID<Structures_Kitchen>(kitchenStationID);

            foreach (StructureBrain structureBrain in StructureManager.StructuresAtLocation(FollowerLocation.Base))
            {
                if (structureBrain is ChefDeskStructure && !structureBrain.ReservedForTask)
                {
                    this.chefDeskID = structureBrain.Data.ID;
                    this.chefDeskStructure = StructureManager.GetStructureByID<ChefDeskStructure>(chefDeskID);
                    return;
                }

            }

            this.End();
            
        }

        public override int GetSubTaskCode() => this.kitchenStationID;

        public override void ClaimReservations()
        {
            if (this.chefDeskStructure == null)
                return;
            this.chefDeskStructure.ReservedForTask = true;
        }

        public override void ReleaseReservations()
        {
            if (this.chefDeskStructure == null)
                return;
            this.chefDeskStructure.ReservedForTask = false;
        }
        
        public override void OnStart() => this.SetState(FollowerTaskState.GoingTo);
        
        public override void TaskTick(float deltaGameTime)
        {
            if (this.State != FollowerTaskState.Doing || (this.kitchenStructure.Data.QueuedMeals.Count <= 0 && this.kitchenStructure.Data.CurrentCookingMeal == null))
            {
                return;
            }
            
            if (this.kitchenStructure.Data.CurrentCookingMeal == null)
            {
                this.kitchenStructure.Data.CurrentCookingMeal = this.kitchenStructure.Data.QueuedMeals[0];
                if (Interaction_Kitchen.Kitchens.Count > 0) //turn on the fire
                {
                    Plugin.Log.LogInfo("turning on fire");
                    Interaction_Kitchen.Kitchens[0].ShowCooking(true);
                }
            }

            else if (this.kitchenStructure.Data.CurrentCookingMeal.CookedTime >= this.kitchenStructure.Data.CurrentCookingMeal.CookingDuration)
            {
                if (followerChef != null)
                {
                    followerChef.TimedAnimation("Reactions/react-happy1", 2.1f, (System.Action)(() => {
                        followerChef.FacePosition(this.kitchenStructure.Data.Position);
                        followerChef.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
                        followerChef.SetBodyAnimation("cook", true);
                        this.ProgressTask();
                    }));
                    this.MealFinishedCooking();
                }
            }

            else
            {
                if (_dissentBubbleCoroutine == null && followerChef != null)
                {
                    _dissentBubbleCoroutine = followerChef.StartCoroutine(DissentBubbleRoutine(followerChef));
                }
                
                this.kitchenStructure.Data.CurrentCookingMeal.CookedTime += deltaGameTime * this._brain.Info.ProductivityMultiplier;
            }
        }

        private void MealFinishedCooking()
        {
            ++DataManager.Instance.MealsCooked;
            ObjectiveManager.CheckObjectives(Objectives.TYPES.COOK_MEALS);

            Structures_FoodStorage availableFoodStorage = Structures_FoodStorage.GetAvailableFoodStorage(this.kitchenStructure.Data.Position, this.Location);
            if (availableFoodStorage != null)
            {
                availableFoodStorage.DepositItemUnstacked(this.kitchenStructure.Data.CurrentCookingMeal.MealType);
            }
            else
            {
                Plugin.Log.LogInfo("No Food Storage?");
                StructureBrain.TYPES mealStructureType = StructuresData.GetMealStructureType(this.kitchenStructure.Data.CurrentCookingMeal.MealType);
                Vector3 position = this.kitchenStructure.Data.Position + (Vector3)Random.insideUnitCircle * 2f;
                StructureManager.BuildStructure(this.kitchenStructure.Data.Location, StructuresData.GetInfoByType(mealStructureType, 0), position, Vector2Int.one);
            }
            
            CookingData.CookedMeal(this.kitchenStructure.Data.CurrentCookingMeal.MealType);
            ObjectiveManager.CheckObjectives(Objectives.TYPES.COOK_MEALS);
            this.kitchenStructure.Data.QueuedMeals.Remove(this.kitchenStructure.Data.CurrentCookingMeal);
            this.kitchenStructure.Data.CurrentCookingMeal = null;
            
            if (this.kitchenStructure.Data.QueuedMeals.Count <= 0)
            {
                //Plugin.Log.LogInfo("Closing bubble");
                if (_dissentBubbleCoroutine != null)
                {
                    followerChef.WorshipperBubble.StopCoroutine(_dissentBubbleCoroutine);
                    followerChef.WorshipperBubble.Close();
                    _dissentBubbleCoroutine = null;
                    Plugin.Log.LogInfo("closed bubble");
                }

                if (Interaction_Kitchen.Kitchens.Count > 0) //turn off the fire
                {
                    Interaction_Kitchen.Kitchens[0].ShowCooking(false);
                }
                this.Complete();
                
            }
        }


        public override void Setup(Follower follower)
        {
            base.Setup(follower);
            if (this.kitchenStationID == 0)
                return;
            follower.SetHat(HatType.Chef);

            followerChef = follower;
            _dissentBubbleCoroutine = follower.StartCoroutine(DissentBubbleRoutine(follower));
        }

        public override void OnDoingBegin(Follower follower)
        {
            if (this.kitchenStationID == 0)
                follower.SetHat(HatType.Chef);
            follower.FacePosition(this.kitchenStructure.Data.Position);
            follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
            follower.SetBodyAnimation("cook", true);

            followerChef = follower;
            if (_dissentBubbleCoroutine == null && followerChef != null)
                _dissentBubbleCoroutine = followerChef.StartCoroutine(DissentBubbleRoutine(followerChef));
        }

        public override void Cleanup(Follower follower)
        {
            follower.SetHat(HatType.None);
            base.Cleanup(follower);

            if (_dissentBubbleCoroutine != null)
            {
                follower.WorshipperBubble.StopCoroutine(_dissentBubbleCoroutine);
                _dissentBubbleCoroutine = null;
                follower.WorshipperBubble.Close();
            }
            followerChef = null;
        }

        private Interaction_Kitchen FindKitchen()
        {
            foreach (Interaction_Kitchen kitchen in Interaction_Kitchen.Kitchens)
            {
                if (kitchen.structure.Structure_Info.ID == this.kitchenStationID)
                    return kitchen;
            }
            return (Interaction_Kitchen)null;
        }

        private static IEnumerator DissentBubbleRoutine(Follower follower) //for the bubble
        {
            float bubbleTimer = 0.3f;
            while (true)
            {
                if ((double)(bubbleTimer -= Time.deltaTime) < 0.0)
                {
                    WorshipperBubble.SPEECH_TYPE Type = WorshipperBubble.SPEECH_TYPE.FOOD;
                    follower.WorshipperBubble.gameObject.SetActive(true);
                    follower.WorshipperBubble.Play(Type);
                    bubbleTimer = (4 + UnityEngine.Random.Range(0, 2));
                }
                yield return null;
            }
        }
    }
}
