﻿using COTL_API.CustomTasks;
using CotLMiniMods.Structures.Proxies;
using CotLTemplateMod;
using CotLTemplateMod.CustomFollowerCommands;
using Spine;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
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
        public SimFollower simvictim;
        public Follower waiter;

        public bool sendingFood = false;
        public bool backToKitchen = false;
        public int _resourceStationId;
        public WaiterDeskStructure _resourceStation;

        public override bool BlockSocial => true;
        public override bool BlockReactTasks => true;
        public override bool BlockTaskChanges => true;

        public override float Priorty => 20f;
        public override Vector3 UpdateDestination(Follower follower)
        {
            if (nextMeal != null && victim != null) return victim.Brain.LastPosition;
            /*if (nextMeal != null && simvictim != null) return simvictim.Brain.LastPosition;*/
            if (nextMeal != null) return follower.Brain.LastPosition;//return this.nextMeal.Data.Position;
            else if (nextMeal == null && Interaction_FollowerKitchen.Kitchens.Count > 0) return Interaction_FollowerKitchen.Kitchens[0].Position - (Vector3)Random.insideUnitCircle * 2f;
            else return base.UpdateDestination(follower);
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
                    return PriorityCategory.Low;
                case FollowerRole.Chef:
                    return PriorityCategory.WorkPriority;
                default:
                    return PriorityCategory.Low;
            }
        }

        public override void Setup(Follower follower)
        {
            base.Setup(follower);
            follower.SetHat(FollowerHatType.Chef);
            follower.SimpleAnimator.ChangeStateAnimation(StateMachine.State.Moving, "Food/food-run");
            waiter = follower;
        }

        public override void OnStart()
        {
            TimeManager.OnNewPhaseStarted += new System.Action(this.OnNewPhaseStarted);
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
                waiter.SetHat(FollowerHatType.None);
                waiter.SimpleAnimator.ResetAnimationsToDefaults();
                waiter.OverridingOutfit = false;
                this.End();
            }
        }

        public override void OnDoingBegin(Follower follower)
        {
            if (sendingFood) //arived at the follower
            {
                //if still too far, such as the other follower moved away
                var pos = victim ? victim.transform.position : follower.transform.position;
                var directid = victim ? victim.Brain._directInfoAccess.ID : simvictim.Brain._directInfoAccess.ID;
                float num = Vector3.Distance(pos, follower.transform.position);
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

                        if (directid == follower.Brain._directInfoAccess.ID)
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
                                ResourceCustomTarget.Create(follower.gameObject, pos, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
                                InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 2, pos);

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
                        simvictim = null;
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
                        simvictim = null;
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
            if (nextMeal != null)
            {
                nextMeal.ReservedForTask = false; //prevent softlocking the food for followers
            }

            if (_resourceStation == null)
                return;
            _resourceStation.ReservedForTask = false;

            
        }

        public void OnNewPhaseStarted()
        {
            this.End();
        }

        public override void Cleanup(Follower follower)
        {
            follower.SetHat(FollowerHatType.None);
            follower.SimpleAnimator.ResetAnimationsToDefaults();
            follower.OverridingOutfit = false;
            base.Cleanup(follower);
        }

        //set the next victim of the meal, can be self, but if it is self the waiter task might go away
        private void FindNextMealVictim()
        {
            Plugin.Log.LogInfo("finding next victim start");
            foreach (ObjectivesData objective in DataManager.Instance.Objectives)
            {
                if (objective is Objectives_EatMeal && ((Objectives_EatMeal)objective).MealType == nextMeal.Data.Type)
                {
                    Plugin.Log.LogInfo("follower by id " + objective.Follower);

                    victim = FollowerManager.FindFollowerByID(objective.Follower);

                }
            }

            if (victim == null)
            {//TODO: make it so that they dont dunk on the same person
                FollowerBrain fb = FollowerManager.GetHungriestFollowerBrain();
                
                //TODO: may become sim follower when fighting, so all follower tasks need to change to simfollower
                victim = FollowerManager.FindFollowerByID(fb._directInfoAccess.ID);
                if (victim == null)
                {
                    Plugin.Log.LogInfo("Could not find victim, using sim victim");
                    simvictim = FollowerManager.FindSimFollowerByID(fb._directInfoAccess.ID);
                    Plugin.Log.LogInfo("sim victim found = " + simvictim.Brain._directInfoAccess.Name);

                }
                else
                {
                    Plugin.Log.LogInfo("victim found = " + victim.Brain._directInfoAccess.Name);
                }

            }
            else
            {
                Plugin.Log.LogInfo("victim found = " + victim.Brain._directInfoAccess.Name);
            }

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
                foreach (Interaction_FollowerKitchen followerKitchen in Interaction_FollowerKitchen.FollowerKitchens)
                {
                    
                    foreach (InventoryItem inventoryItem in followerKitchen.foodStorage.StructureInfo.Inventory)
                    {
                        if (inventoryItem.UnreservedQuantity > 0)
                        {
                            Plugin.Log.LogInfo("Found stored food of type " + (InventoryItem.ITEM_TYPE)inventoryItem.type);
                            followerKitchen.foodStorage.StructureBrain.TryClaimFoodReservation((InventoryItem.ITEM_TYPE)inventoryItem.type);
                            if (followerKitchen.foodStorage.StructureBrain.TryEatReservedFood((InventoryItem.ITEM_TYPE)inventoryItem.type))
                            {
                                Plugin.Log.LogInfo("removed stored food");
                                StructureBrain.TYPES mealStructureType = StructuresData.GetMealStructureType((InventoryItem.ITEM_TYPE)inventoryItem.type);
                                Vector3 position = this._resourceStation.Data.Position + (Vector3)Random.insideUnitCircle * 2f;
                                StructureManager.BuildStructure(this._resourceStation.Data.Location, StructuresData.GetInfoByType(mealStructureType, 0), position, Vector2Int.one);
                                break;
                            }

                        }
                    }

                }
            }
        }
    }
}
