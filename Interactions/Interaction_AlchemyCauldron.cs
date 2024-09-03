using CotLTemplateMod;
using Lamb.UI.FollowerSelect;
using Lamb.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using src.Extensions;
using Lamb.UI.FollowerInteractionWheel;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using CotLMiniMods.Structures.Proxies;
using CotLMiniMods.Structures;
using System.Collections;
using UnityEngine.XR;

namespace CotLMiniMods.Interactions
{
    internal class Interaction_AlchemyCauldron : Interaction
    {
        public Structure Structure;
        private bool Activated;
        public StructuresData StructureInfo => this.Structure.Structure_Info;
        public Structures_AlchemyCauldron cCauldron => this.Structure.Brain as Structures_AlchemyCauldron;

        public int CurrentSuccesses = 0;
        public int maxSuccesses = 20;

        public UICookingMinigameOverlayController _uiCookingMinigameOverlayController;

        public override void GetLabel()
        {
            this.label = "5x Strange Material: Select a Necklace";
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Plugin.Log.LogInfo("I am enabled Alchemy Cauldron");
            Structure = GetComponentInParent<Transform>().GetComponent<Structure>();

        }

        public override void OnInteract(StateMachine state)
        {
            if (this.Activated) return;

            if (Inventory.GetItemQuantity(Plugin.StrangeMaterialItem) >= 5)
            {
                this.Activated = true;
                List<InventoryItem.ITEM_TYPE> QuarryItems = this.cCauldron.CookItems;
                state.CURRENT_STATE = StateMachine.State.InActive;
                state.facingAngle = Utils.GetAngle(state.transform.position, this.transform.position);
                CameraFollowTarget cameraFollowTarget = CameraFollowTarget.Instance;
                cameraFollowTarget.SetOffset(new Vector3(0.0f, 4.5f, 2f));
                cameraFollowTarget.AddTarget(this.gameObject, 1f);
                HUD_Manager.Instance.Hide(false, 0);

                UIItemSelectorOverlayController itemSelector = MonoSingleton<UIManager>.Instance.ShowItemSelector(this.playerFarming, QuarryItems, new ItemSelector.Params()
                {
                    Key = "cCauldron",
                    Context = ItemSelector.Context.SetLabel,
                    Offset = new Vector2(0.0f, 150f),
                    ShowEmpty = true,
                    RequiresDiscovery = false,
                    HideQuantity = true,
                    HideOnSelection = true,
                });
                itemSelector.OnItemChosen += chosenItem =>
                {
                    Plugin.Log.LogInfo("item selected " + chosenItem);
                    this.cCauldron.SelectedCookItem = chosenItem;
                    Inventory.ChangeItemQuantity(Plugin.StrangeMaterialItem, -5);
                    ResourceCustomTarget.Create(this.gameObject, PlayerFarming.Instance.transform.position, Plugin.StrangeMaterialItem, null);
                    state.CURRENT_STATE = StateMachine.State.InActive;
                    GameManager.GetInstance().OnConversationNew();
                    GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.CameraBone);
                    
                    PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
                    PlayerFarming.Instance.Spine.UseDeltaTime = false;
                    PlayerFarming.Instance.Spine.skeleton.Update(Time.deltaTime);
                    PlayerFarming.Instance.simpleSpineAnimator.Animate("actions/dig", 0, true);

                    /*PlayerFarming.Instance.GoToAndStop(this.StructureInfo.Position + new Vector3(0.1f, 2.5f), this.transform.parent.gameObject, GoToCallback: ((System.Action)(() =>
                    {*/
                    var tempQueueMeal = new Interaction_Kitchen.QueuedMeal();
                    tempQueueMeal.MealType = this.cCauldron.SelectedCookItem;
                    this.StructureInfo.QueuedMeals.Clear();
                    this.maxSuccesses = this.cCauldron.CookItems.IndexOf(this.cCauldron.SelectedCookItem) + 1;

                    for (int i = 0; i < this.cCauldron.CookItems.IndexOf(this.cCauldron.SelectedCookItem) + 1; i++)
                    {
                        this.StructureInfo.QueuedMeals.Add(tempQueueMeal);
                    }
                    
                    try
                    {
                        _uiCookingMinigameOverlayController = MonoSingleton<UIManager>.Instance.CookingMinigameOverlayControllerTemplate.Instantiate<UICookingMinigameOverlayController>();
                        _uiCookingMinigameOverlayController.Initialise(this.StructureInfo, Interaction_Kitchen.Kitchens[0]);
                        _uiCookingMinigameOverlayController.OnCook += new System.Action(this.SuccessfulMinigame);
                        _uiCookingMinigameOverlayController.OnUnderCook += new System.Action(this.FailedMinigame);
                        _uiCookingMinigameOverlayController.OnBurn += new System.Action(this.FailedMinigame);
                    }
                    catch (Exception e)
                    {
                        Plugin.Log.LogInfo("You need a cooking fire or a follower kitchen first to cook necklaces too, you know?");
                    }

                    /*})));*/
                    HUD_Manager.Instance.Show(0);
                };
                itemSelector.OnCancel += () =>
                {
                    HUD_Manager.Instance.Show(0);
                    state.CURRENT_STATE = StateMachine.State.Idle;
                };
                itemSelector.OnHidden += () =>
                {
                    cameraFollowTarget.SetOffset(Vector3.zero);
                    cameraFollowTarget.RemoveTarget(this.gameObject);
                    /*state.CURRENT_STATE = StateMachine.State.Idle;*/
                    itemSelector = null;
                    this.Interactable = true;
                    this.HasChanged = true;
                };

                //generate a random necklace
                this.Activated = false;
            }
            
        }

        public void SuccessfulMinigame()
        {
            //the selected necklace requires x amount of successes, where x is the index of the necklace.
            CurrentSuccesses++;
            Plugin.Log.LogInfo("Succeeded " + CurrentSuccesses + " / " + maxSuccesses + " times");

            if (this.StructureInfo.QueuedMeals.Count > 1)
            {
                this.StructureInfo.QueuedMeals.RemoveAt(0);
            }

            if (CurrentSuccesses >= maxSuccesses)
            {
                PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
                PlayerFarming.Instance.Spine.UseDeltaTime = false;
                PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
                PlayerFarming.Instance.Spine.UseDeltaTime = false;
                PlayerFarming.Instance.Spine.skeleton.Update(Time.deltaTime);
                PlayerFarming.Instance.simpleSpineAnimator.Animate("reactions/react-happy2", 0, false);
                
                
                InventoryItem.Spawn(this.cCauldron.SelectedCookItem, 1, this.Position);
                this.StartCoroutine(this.EndCooking());
            }

        }

        public void FailedMinigame()
        {
            PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
            PlayerFarming.Instance.Spine.UseDeltaTime = false;
            PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
            PlayerFarming.Instance.Spine.UseDeltaTime = false;
            PlayerFarming.Instance.Spine.skeleton.Update(Time.deltaTime);
            PlayerFarming.Instance.simpleSpineAnimator.Animate("reactions/react-angry", 0, false);
            
            HealthPlayer component = PlayerFarming.Instance.GetComponent<HealthPlayer>();
            AudioManager.Instance.PlayOneShot("event:/player/gethit", PlayerFarming.Instance.gameObject);
            component.HP -= 2f;
            //hurt the player
            this.StartCoroutine(this.EndCooking());
        }

        private IEnumerator EndCooking()
        {
            Plugin.Log.LogInfo("ending cooking");
            this._uiCookingMinigameOverlayController.OnCook -= new System.Action(this.SuccessfulMinigame);
            this._uiCookingMinigameOverlayController.OnUnderCook -= new System.Action(this.FailedMinigame);
            this._uiCookingMinigameOverlayController.OnBurn -= new System.Action(this.FailedMinigame);
            this._uiCookingMinigameOverlayController.Close();
            this._uiCookingMinigameOverlayController = null;

            yield return new WaitForSeconds(2.4f);
            Plugin.Log.LogInfo("ending animation");
            GameManager.GetInstance().OnConversationEnd();
            PlayerFarming.Instance.Spine.UseDeltaTime = true;
            CurrentSuccesses = 0;


        }


    }
}
