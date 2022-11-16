using COTL_API.Tasks;
using CotLMiniMods.Structures;
using CotLTemplateMod;
using CotLTemplateMod.CustomFollowerCommands;
using Spine;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CotLMiniMods.CCommands.Tasks
{
    //TODO: Waiter v2
    //The waiter shall be given tips (2 - 6 gold based on the food type)
    //The waiter shall now teleport the food to the follower instead of walking back to the kitchen to take food
    //Food served by waiters doubles the satiation amount
    internal class WaiterTask : CustomTask
    {
        public override string InternalName => "Waiter_Task";

        public Structures_Meal nextMeal;
        public Follower victim;
        public Follower waiter;

        public bool sendingFood = false;
        public bool backToKitchen = false;
        public int _resourceStationId;
        public WaiterDeskStructure _resourceStation;
        /*public override bool BlockTaskChanges => true;*/
        public override Vector3 UpdateDestination(Follower follower)
        {
            if (nextMeal != null && victim != null) return victim.Brain.LastPosition;
            if (nextMeal != null) return follower.Brain.LastPosition;//return this.nextMeal.Data.Position;
            else if (nextMeal == null && Interaction_Kitchen.Kitchens.Count > 0) return Interaction_Kitchen.Kitchens[0].Position - (Vector3)Random.insideUnitCircle * 2f;
            else return base.UpdateDestination(follower);
        }

        public override void Setup(Follower follower)
        {
            base.Setup(follower);
            follower.SetHat(HatType.Chef);
            follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Food/food-run"); //temporary or Farming/run-berries Food/food_eat
            waiter = follower;
        }

        public override void OnStart()
        {
            //this.SetState(FollowerTaskState.WaitingForLocation);
            SetState(FollowerTaskState.GoingTo);
        }

        public override void TaskTick(float deltaGameTime)
        {
            if (nextMeal == null) //loop for finding meals
            {
                FindNextMeal();
            }

            if (nextMeal == null) //if still null, then end
            {
                Plugin.Log.LogInfo("no more next meal");
                waiter.SetHat(HatType.None);
                waiter.SimpleAnimator.ResetAnimationsToDefaults();
                waiter.OverridingOutfit = false;
                Complete();
            }
        }

        public override void OnDoingBegin(Follower follower)
        {
            if (sendingFood) //arived at the follower
            {
                //if still too far, such as the other follower moved away
                float num = Vector3.Distance(victim.transform.position, follower.transform.position);
                if ((double)num > 7.0) //sometimes chasing breaks the waiter so a bigger acceptable radius is easier on the task
                {
                    Plugin.Log.LogInfo("victim moved, chasing..");
                    ClearDestination();
                    SetState(FollowerTaskState.GoingTo);
                }
                else
                {
                    follower.TimedAnimation("Farming/add-berries", 0.9166667f, () => //temporary anim or Farming/add-berries Buildings/add-stone
                    {
                        Plugin.Log.LogInfo("dunked food on victim");
                        follower.SimpleAnimator.ResetAnimationsToDefaults();
                        follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Idle, "dance");

                        Plugin.Log.LogInfo("forcing victim to eat..");

                        if (victim.Brain._directInfoAccess.ID == follower.Brain._directInfoAccess.ID)
                        {
                            Plugin.Log.LogInfo("Self eating");
                            _brain.Stats.Satiation += CookingData.GetSatationAmount(CookingData.GetMealFromStructureType(nextMeal.Data.Type)) * 2;
                            nextMeal.Data.Eaten = true;
                        }
                        else
                        {
                            try
                            {
                                //extra bonus satiation for being served
                                _brain.Stats.Satiation += CookingData.GetSatationAmount(CookingData.GetMealFromStructureType(nextMeal.Data.Type));

                                //free gold
                                ResourceCustomTarget.Create(follower.gameObject, victim.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
                                InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 2, victim.transform.position);

                                //eat meal
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
                        FindNextMeal();

                    });
                }

            }
            else //grab the food
            {
                if (nextMeal == null)
                {
                    Plugin.Log.LogInfo("no meal available");
                    if (_currentDestination.HasValue && !backToKitchen) //go back to base and wait for food
                    {
                        backToKitchen = true;
                        ClearDestination();
                        SetState(FollowerTaskState.GoingTo);
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
                        SetState(FollowerTaskState.GoingTo);
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
                    follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Food/food-run"); //temporary or Farming/run-berries Food/food_eat

                    //FindNextMealVictim();
                    if (_currentDestination.HasValue) //clear if have destination
                    {
                        ClearDestination();
                    }
                    SetState(FollowerTaskState.GoingTo);

                }
            }
        }

        public WaiterTask()
        {
            foreach (StructureBrain structureBrain in StructureManager.StructuresAtLocation(FollowerLocation.Base))
            {
                if (structureBrain is WaiterDeskStructure && !structureBrain.ReservedForTask)
                {
                    _resourceStationId = structureBrain.Data.ID;
                    _resourceStation = StructureManager.GetStructureByID<WaiterDeskStructure>(_resourceStationId);
                    FindNextMeal();

                    return;
                }

            }
            Complete();

        }

        public WaiterTask(int id)
        {
            _resourceStationId = id;
            _resourceStation = StructureManager.GetStructureByID<WaiterDeskStructure>(_resourceStationId);
            FindNextMeal();
        }

        public override void ClaimReservations()
        {

            if (_resourceStation == null)
                return;
            _resourceStation.ReservedForTask = true;
        }

        public override void ReleaseReservations()
        {

            if (_resourceStation == null)
                return;
            _resourceStation.ReservedForTask = false;
        }

        public override void Cleanup(Follower follower)
        {
            follower.SetHat(HatType.None);
            follower.SimpleAnimator.ResetAnimationsToDefaults();
            follower.OverridingOutfit = false;
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
            {//TODO: make it so that they dont dunk on the same person
                FollowerBrain fb = FollowerManager.GetHungriestFollowerBrain();
                victim = FollowerManager.FindFollowerByID(fb._directInfoAccess.ID);
            }
            Plugin.Log.LogInfo("victim found = " + victim.Brain._directInfoAccess.Name);
            if (_currentDestination.HasValue) //clear if have destination
            {
                ClearDestination();
                SetState(FollowerTaskState.GoingTo);
            }

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
                        Plugin.Log.LogInfo("found food, finding a next victim");
                        nextMeal = structuresMeal;
                        backToKitchen = false;

                        if (victim == null)
                            FindNextMealVictim();


                        if (_currentDestination.HasValue) //clear if have destination
                        {
                            ClearDestination();
                            SetState(FollowerTaskState.GoingTo);
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
                foreach (Structures_FoodStorage structuresFoodStorage in StructureManager.GetAllStructuresOfType<Structures_FoodStorage>(Location))
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
