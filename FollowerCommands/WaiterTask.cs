using COTL_API.Tasks;
using Spine;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CotLTemplateMod.CustomFollowerCommands
{
    internal class WaiterTask : CustomTask
    {
        public override string InternalName => "Waiter_Task";

        public Structures_Meal nextMeal;
        public Follower victim;
        public bool sendingFood = false;
        public bool backToKitchen = false;

        public override bool BlockTaskChanges => true;
        public override Vector3 UpdateDestination(Follower follower)
        {
            if (nextMeal != null && victim != null) return this.victim.Brain.LastPosition;
            if (nextMeal != null) return this.nextMeal.Data.Position;
            else if (nextMeal == null && Interaction_Kitchen.Kitchens.Count > 0) return Interaction_Kitchen.Kitchens[0].Position - ((Vector3)Random.insideUnitCircle * 2f);
            else return base.UpdateDestination(follower);
        }

        public override void Setup(Follower follower)
        {
            base.Setup(follower);
            follower.SetHat(HatType.Chef);
        }

        public override void OnStart()
        {
            //this.SetState(FollowerTaskState.WaitingForLocation);
            this.SetState(FollowerTaskState.GoingTo);
        }

        public override void TaskTick(float deltaGameTime)
        {
            if (nextMeal == null) //loop for finding meals
            {
                FindNextMeal();
            }
        }

        public override void OnDoingBegin(Follower follower)
        {
            if (sendingFood) //arived at the follower
            {
                //if still too far, such as the other follower moved away
                float num = Vector3.Distance(victim.transform.position, follower.transform.position);
                if ((double)num > 10.0) //sometimes chasing breaks the waiter so a bigger acceptable radius is easier on the task
                {
                    Plugin.Log.LogInfo("victim moved, chasing..");
                    this.ClearDestination();
                    this.SetState(FollowerTaskState.GoingTo);
                }
                else
                {
                    follower.TimedAnimation("Buildings/add-stone", 0.9166667f, () => //temporary anim or Farming/add-berries
                    {
                        Plugin.Log.LogInfo("dunked food on victim");
                        follower.SimpleAnimator.ResetAnimationsToDefaults();
                        follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "dance");

                        Plugin.Log.LogInfo("forcing victim to eat..");

                        if (victim.Brain._directInfoAccess.ID == follower.Brain._directInfoAccess.ID)
                        {
                            Plugin.Log.LogInfo("Self eating");
                            this._brain.Stats.Satiation += CookingData.GetSatationAmount(CookingData.GetMealFromStructureType(nextMeal.Data.Type));
                            nextMeal.Data.Eaten = true;
                        }
                        else
                        {
                            try
                            {
                                victim.Brain.HardSwapToTask(new EatWaiterTask(nextMeal.Data.ID));

                            }
                            catch (Exception e)
                            {
                                Plugin.Log.LogInfo("Food may have been eaten already");
                            }
                        }

                        //now go back and find food again
                        nextMeal = null;
                        victim = null;
                        sendingFood = false;
                        this.SetState(FollowerTaskState.GoingTo);

                    });
                }
                
            }
            else //grab the food
            {
                if (nextMeal == null)
                {
                    Plugin.Log.LogInfo("no meal available");
                    if (this._currentDestination.HasValue && !backToKitchen) //go back to base and wait for food
                    {
                        backToKitchen = true;
                        this.ClearDestination();
                        this.SetState(FollowerTaskState.GoingTo);
                    }
                }

                else
                {
                    if (nextMeal.ReservedForTask) //skip if another waiter has taken it
                    {
                        Plugin.Log.LogInfo("another waiter got to it");
                        nextMeal = null;
                        victim = null;
                        sendingFood = false;
                        this.SetState(FollowerTaskState.GoingTo);
                        return;
                    }

                    nextMeal.ReservedForTask = true; //have to find a way to hide the food from the floor?

                    follower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
                    //follower.SetBodyAnimation("Food/food_eat", true);
                    follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "dance");
                    //the mealskin cant be seen on other animations(?), find a way to show the meal on hand

                    Plugin.Log.LogInfo("arrived at food");
                    sendingFood = true;

                    Skin newSkin = new Skin("MealSkin");
                    newSkin.AddSkin(follower.Spine.skeleton.Skin);
                    newSkin.AddSkin(follower.Spine.Skeleton.Data.FindSkin(CookingData.GetMealSkin(nextMeal.Data.Type)));
                    follower.OverridingOutfit = true;
                    follower.Spine.skeleton.SetSkin(newSkin);
                    follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Food/food_eat"); //temporary or Farming/run-berries

                    FindNextMealVictim();

                    
                }
            }
        }

        public WaiterTask()
        {
            FindNextMeal();
        }

        public override void Cleanup(Follower follower)
        {
            follower.SetHat(HatType.None);
            follower.SimpleAnimator.ResetAnimationsToDefaults();
            base.Cleanup(follower);
        }

        //set the next victim of the meal, can be self, but if it is self the waiter task might go away
        private void FindNextMealVictim()
        {
            foreach (ObjectivesData objective in DataManager.Instance.Objectives)
            {
                if (objective is Objectives_EatMeal && ((Objectives_EatMeal)objective).MealType == nextMeal.Data.Type)
                {
                    victim = FollowerManager.FindFollowerByID(objective.Follower);
                    
                }
            }
            if (victim == null)
            {
                FollowerBrain fb = FollowerManager.GetHungriestFollowerBrain();
                victim = FollowerManager.FindFollowerByID(fb._directInfoAccess.ID);
            }
            Plugin.Log.LogInfo("victim found = " + victim.Brain._directInfoAccess.Name);
            this.ClearDestination();
            this.SetState(FollowerTaskState.GoingTo);

        }

        //find valid food to collect
        private void FindNextMeal()
        {
            //from floor
            foreach (Structures_Meal structuresMeal in StructureManager.GetAllStructuresOfType<Structures_Meal>(FollowerLocation.Base))
            {
                if (!structuresMeal.Data.Rotten && !structuresMeal.Data.Burned && !structuresMeal.ReservedForTask)
                {
                    if (nextMeal == null)
                    {
                        Plugin.Log.LogInfo("found food, walking to food");
                        nextMeal = structuresMeal;
                        backToKitchen = false;

                        if (this._currentDestination.HasValue) //clear if have destination
                        {
                            this.ClearDestination();
                            this.SetState(FollowerTaskState.GoingTo);
                        }
                        

                        //itemType = CookingData.GetMealFromStructureType(structuresMeal.Data.Type);
                        break;
                    }
                }
            }

            if (nextMeal == null) //pull out from storage, then find again.
            {
                //from the food storage
                //TODO
                foreach (Structures_FoodStorage structuresFoodStorage in StructureManager.GetAllStructuresOfType<Structures_FoodStorage>(this.Location))
                {
                    foreach (InventoryItem inventoryItem in structuresFoodStorage.Data.Inventory)
                    {
                        if (inventoryItem.UnreservedQuantity > 0)
                        {
                            Plugin.Log.LogInfo("Found stored food of type " + (InventoryItem.ITEM_TYPE)inventoryItem.type);
                            structuresFoodStorage.TryClaimFoodReservation((InventoryItem.ITEM_TYPE)inventoryItem.type);
                            if (structuresFoodStorage.TryEatReservedFood((InventoryItem.ITEM_TYPE)inventoryItem.type))
                            {
                                Plugin.Log.LogInfo("removed stored food");
                                StructureBrain.TYPES mealStructureType = StructuresData.GetMealStructureType((InventoryItem.ITEM_TYPE)inventoryItem.type);
                                Vector3 position = structuresFoodStorage.Data.Position + (Vector3)Random.insideUnitCircle * 2f;
                                StructureManager.BuildStructure(structuresFoodStorage.Data.Location, StructuresData.GetInfoByType(mealStructureType, 0), position, Vector2Int.one);
                                break;
                            }
                            
                        }
                    }
                    
                }
            }
        }
    }
}
