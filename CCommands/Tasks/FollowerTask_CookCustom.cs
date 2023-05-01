using COTL_API.Tasks;
using CotLMiniMods.Structures.Proxies;
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
        public override int UsingStructureID => this.chefDeskID;
        public override bool BlockSocial => true;
        public override bool BlockTaskChanges => true;
        public override FollowerLocation Location => this.chefDeskStructure.Data.Location;
        public override float Priorty => 20f;

        private ChefDeskStructure chefDeskStructure;
        private Structures_Kitchen kitchenStructure;
        private int chefDeskID;
        public override Vector3 UpdateDestination(Follower follower) => this.chefDeskStructure.Data.Position + new Vector3(0.0f, 0.521f);

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

        public FollowerTask_CookCustom(int desk)
        {
            this.chefDeskID = desk;
            this.chefDeskStructure = StructureManager.GetStructureByID<ChefDeskStructure>(desk);
            FindKitchen();
        }

        public FollowerTask_CookCustom()
        {
            foreach (StructureBrain structureBrain in StructureManager.StructuresAtLocation(FollowerLocation.Base))
            {
                if (structureBrain is ChefDeskStructure && !structureBrain.ReservedForTask)
                {
                    this.chefDeskID = structureBrain.Data.ID;
                    this.chefDeskStructure = StructureManager.GetStructureByID<ChefDeskStructure>(chefDeskID);
                    FindKitchen();
                    return;
                }

            }
            this.End();
        }

        public override int GetSubTaskCode() => this.chefDeskID;

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
            bool flag = false;
            //there are two types of tasks,
            //task 1 is that the signature dish, when assigned, can be cooked on demand when followers request
            //the cost is 1 strange material per dish cooked, and the dish is cooked in 10 seconds. if the follower kitchen is full, the dish is not cooked
            //
            //task 2 is that when a follower is cooking in the kitchen, they provide moral support and cook in the counter as well

            //interactions: left click to deposit strange material, right click to select dish, another click to show upgrade screen
            if (this.State != FollowerTaskState.Doing)
            {
                return;
            }

            //check if the current task is to aid the cook in follower kitchen or to cook the signature dish
            if (this.kitchenStructure != null)
            {
                //aid
                if (this.kitchenStructure.Data.CurrentCookingMeal == null)
                {
                    Interaction_Kitchen.QueuedMeal bestPossibleMeal = this.kitchenStructure.GetBestPossibleMeal();
                    if (bestPossibleMeal == null)
                        flag = true;
                    else
                        this.kitchenStructure.Data.CurrentCookingMeal = bestPossibleMeal;
                }

                if (!flag) {
                    if (kitchenStructure.Data.CurrentCookingMeal.CookedTime >= kitchenStructure.Data.CurrentCookingMeal.CookingDuration)
                    {
                        this.MealFinishedCooking();
                    }
                    else
                    {
                        //include efficiency, etc
                        this.kitchenStructure.Data.CurrentCookingMeal.CookedTime += deltaGameTime * this._brain.Info.ProductivityMultiplier;

                    }
                }
            }
            else
            {
                flag = true;
            }
            
            if (flag)
            {
               /* Plugin.Log.LogInfo("No kitchen found, we cook ourselves");*/
                //cook self
                this.chefDeskStructure.Data.Progress += deltaGameTime * this._brain.Info.ProductivityMultiplier;

                if (this.chefDeskStructure.Data.Progress < 75)
                    return;

                this.chefDeskStructure.Data.Progress = 0.0f;

                //TODO: also check if there is follower kitchen space
                bool flag1 = false;
                foreach (Interaction_FollowerKitchen k in Interaction_FollowerKitchen.FollowerKitchens)
                {
                    if ((k.foodStorage.StructureBrain.Data.Inventory.Count < k.foodStorage.StructureBrain.Capacity) && k.StructureInfo.CurrentCookingMeal == null)
                    {
                        //store food
                        Plugin.Log.LogInfo("Adding food to kitchen via Sous Chef Desk");
                        k.foodStorage.StructureBrain.DepositItemUnstacked(this.chefDeskStructure.SelectedCookItem);
                        k.foodStorage.UpdateFoodDisplayed();
                        flag1 = true;
                        break;
                    }
                }
                if (!flag1) //if not put into storage, then spawn it on the ground
                {
                    StructureBrain.TYPES mealStructureType = StructuresData.GetMealStructureType(this.chefDeskStructure.SelectedCookItem);
                    Vector3 position = this.chefDeskStructure.Data.Position + (Vector3)Random.insideUnitCircle * 2f;
                    StructureManager.BuildStructure(this.chefDeskStructure.Data.Location, StructuresData.GetInfoByType(mealStructureType, 0), position, Vector2Int.one);
                }
                
                if (followerChef != null)
                {
                    followerChef.TimedAnimation("Reactions/react-happy1", 2.1f, (System.Action)(() => {
                        followerChef.FacePosition(this.chefDeskStructure.Data.Position);
                        followerChef.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
                        followerChef.SetBodyAnimation("cook", true);
                        this.Complete();
                    }));
                }
            }
            
           

            /*else if (this.kitchenStructure.Data.CurrentCookingMeal.CookedTime >= this.kitchenStructure.Data.CurrentCookingMeal.CookingDuration)
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
            }*/

           /* else
            {
                if (_dissentBubbleCoroutine == null && followerChef != null)
                {
                    _dissentBubbleCoroutine = followerChef.StartCoroutine(DissentBubbleRoutine(followerChef));
                }
                
                this.kitchenStructure.Data.CurrentCookingMeal.CookedTime += deltaGameTime * this._brain.Info.ProductivityMultiplier;
            }*/
        }

        private void MealFinishedCooking() //TODO: change this into not dropping food but meal objectives only
        {
            ++DataManager.Instance.MealsCooked;
            ObjectiveManager.CheckObjectives(Objectives.TYPES.COOK_MEALS);

            //as of 1.20, no need food storage anymore, will become sous chef

            /*Structures_FoodStorage availableFoodStorage = GetAvailableFoodStorage(this.kitchenStructure.Data.Position, this.Location);
            if (availableFoodStorage != null)
            {
                availableFoodStorage.DepositItemUnstacked(this.kitchenStructure.Data.CurrentCookingMeal.MealType);
            }
            else
            {*/
                Plugin.Log.LogInfo("No Food Storage?");
                StructureBrain.TYPES mealStructureType = StructuresData.GetMealStructureType(this.kitchenStructure.Data.CurrentCookingMeal.MealType);
                Vector3 position = this.kitchenStructure.Data.Position + (Vector3)Random.insideUnitCircle * 2f;
                StructureManager.BuildStructure(this.kitchenStructure.Data.Location, StructuresData.GetInfoByType(mealStructureType, 0), position, Vector2Int.one);
            /*}*/
            
            CookingData.CookedMeal(this.kitchenStructure.Data.CurrentCookingMeal.MealType);
            ObjectiveManager.CheckObjectives(Objectives.TYPES.COOK_MEALS);
            this.kitchenStructure.Data.QueuedMeals.Remove(this.kitchenStructure.Data.CurrentCookingMeal);
            this.kitchenStructure.Data.CurrentCookingMeal = null;
            
            if (this.kitchenStructure.Data.QueuedMeals.Count <= 0) //TODO: change this
            {
                //Plugin.Log.LogInfo("Closing bubble");
                if (_dissentBubbleCoroutine != null)
                {
                    followerChef.WorshipperBubble.StopCoroutine(_dissentBubbleCoroutine);
                    followerChef.WorshipperBubble.Close();
                    _dissentBubbleCoroutine = null;
                    Plugin.Log.LogInfo("closed bubble");
                }

                this.Complete();
                
            }
        }


        public override void Setup(Follower follower)
        {
            base.Setup(follower);
            follower.SetHat(HatType.Chef);
            follower.SetOutfit(FollowerOutfitType.Undertaker, false);

            followerChef = follower;
            _dissentBubbleCoroutine = follower.StartCoroutine(DissentBubbleRoutine(follower));
        }

        public override void OnDoingBegin(Follower follower)
        {
            follower.SetHat(HatType.Chef);
            follower.SetOutfit(FollowerOutfitType.Undertaker, false);

            follower.FacePosition(this.chefDeskStructure.Data.Position);
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

        private Interaction_FollowerKitchen FindKitchen()
        {
            foreach (Interaction_FollowerKitchen kitchen in Interaction_FollowerKitchen.FollowerKitchens)
            {
                this.kitchenStructure = StructureManager.GetStructureByID<Structures_Kitchen>(kitchen.StructureInfo.ID);
                return kitchen;
            }
            return null;
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
