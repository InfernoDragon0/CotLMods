﻿using CotLTemplateMod;
using Lamb.UI.FollowerSelect;
using Lamb.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using src.Extensions;
using Lamb.UI.FollowerInteractionWheel;
using DG.Tweening;
using CotLMiniMods.Structures.Mines;

namespace CotLMiniMods.Interactions
{
    internal class Interaction_AIOQuarry : Interaction
    {
        public Structure Structure;
        private bool Activating = false;
        private float Delay = 0.04f;
        public float DistanceToTriggerDeposits = 5f;

        public StructuresData StructureInfo => this.Structure.Structure_Info;
        public Structures_AIOQuarry AIOQuarry => this.Structure.Brain as Structures_AIOQuarry;
        public override void GetLabel()
        {
            this.label = "structure was null";
            this.SecondaryLabel = "Select Item to Quarry (Currently " + this.AIOQuarry.SelectedQuarryItem + ")";

            if (this.Structure != null)
                this.label = "Collect Items: x" + this.StructureInfo.Inventory.Count;
        }

        private void Start()
        {
            this.ContinuouslyHold = true;
            this.HasSecondaryInteraction = true;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Plugin.Log.LogInfo("I am enabled AIO Quarry Interaction");
            Structure = GetComponentInParent<Transform>().GetComponent<Structure>();
        }

        public override void OnInteract(StateMachine state)
        {
            if (this.Activating) return;
            base.OnInteract(state);
            this.Activating = true;
        }

        public override void OnSecondaryInteract(StateMachine state)
        {
            base.OnSecondaryInteract(state);
            List<InventoryItem.ITEM_TYPE> QuarryItems = this.AIOQuarry.QuarryItems;
            state.CURRENT_STATE = StateMachine.State.InActive;
            state.facingAngle = Utils.GetAngle(state.transform.position, this.transform.position);
            CameraFollowTarget cameraFollowTarget = CameraFollowTarget.Instance;
            cameraFollowTarget.SetOffset(new Vector3(0.0f, 4.5f, 2f));
            cameraFollowTarget.AddTarget(this.gameObject, 1f);
            HUD_Manager.Instance.Hide(false, 0);
            
            UIItemSelectorOverlayController itemSelector = MonoSingleton<UIManager>.Instance.ShowItemSelector(this.playerFarming, QuarryItems, new ItemSelector.Params()
            {
                Key = "AIOQuarry",
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
                this.AIOQuarry.SelectedQuarryItem = chosenItem;
                HUD_Manager.Instance.Show(0);
            };
            itemSelector.OnCancel += () => HUD_Manager.Instance.Show(0);
            itemSelector.OnHidden += () =>
            {
                cameraFollowTarget.SetOffset(Vector3.zero);
                cameraFollowTarget.RemoveTarget(this.gameObject);
                state.CURRENT_STATE = StateMachine.State.Idle;
                itemSelector = null;
                this.Interactable = true;
                this.HasChanged = true;
            };

        }
        public override void Update()
        {
            base.Update();
            this.GetLabel();

            if (this.Activating && (this.StructureInfo.Inventory.Count <= 0 || InputManager.Gameplay.GetInteractButtonUp(this.playerFarming)))
            {
                this.Activating = false;
            }

            if ((double)(this.Delay -= Time.deltaTime) >= 0.0 || !this.Activating)
                return;

            InventoryItem.ITEM_TYPE itemType = (InventoryItem.ITEM_TYPE)this.StructureInfo.Inventory[0].type;
            AudioManager.Instance.PlayOneShot("event:/followers/pop_in", this.gameObject);
            ResourceCustomTarget.Create(this.state.gameObject, this.transform.position, itemType, () => this.GiveItem(itemType));
            this.StructureInfo.Inventory.RemoveAt(0);
            this.Delay = 0.04f;
        }

        private void GiveItem(InventoryItem.ITEM_TYPE type) => Inventory.AddItem((int)type, 1);
    }
}
