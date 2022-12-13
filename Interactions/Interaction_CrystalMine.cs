using CotLTemplateMod;
using Lamb.UI.FollowerSelect;
using Lamb.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using src.Extensions;
using Lamb.UI.FollowerInteractionWheel;
using DG.Tweening;

namespace CotLMiniMods.Interactions
{
    internal class Interaction_CrystalMine : Interaction
    {
        public Structure Structure;
        private bool Activating = false;
        private GameObject Player;
        private float Delay = 0.04f;
        public float DistanceToTriggerDeposits = 5f;

        public StructuresData StructureInfo => this.Structure.Structure_Info;
        public override void GetLabel()
        {
            int count = this.StructureInfo.Inventory.Count;
            //this.Interactable = count > 0;

            if (this.Structure != null)
                this.label = "Collect Items: x" + this.StructureInfo.Inventory.Count;
            else
                this.label = "structure was null";
        }

        private void Start()
        {
            this.ContinuouslyHold = true;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Plugin.Log.LogInfo("I am enabled CrystalMine Interaction");
            Structure = GetComponentInParent<Transform>().GetComponent<Structure>();
        }

        public override void OnInteract(StateMachine state)
        {
            if (this.Activating) return;
            base.OnInteract(state);
            this.Activating = true;
            
        }
        public override void Update()
        {
            if ((this.Player = GameObject.FindWithTag("Player")) == null)
                return;
            
            this.GetLabel();

            if (this.Activating && (this.StructureInfo.Inventory.Count <= 0 || InputManager.Gameplay.GetInteractButtonUp()))
            {
                this.Activating = false;
            }

            if ((double)(this.Delay -= Time.deltaTime) >= 0.0 || !this.Activating)
                return;

            InventoryItem.ITEM_TYPE itemType = (InventoryItem.ITEM_TYPE)this.StructureInfo.Inventory[0].type;
            AudioManager.Instance.PlayOneShot("event:/followers/pop_in", this.gameObject);
            ResourceCustomTarget.Create(this.state.gameObject, this.transform.position, itemType, (System.Action)(() => this.GiveItem(itemType)));
            this.StructureInfo.Inventory.RemoveAt(0);
            this.Delay = 0.04f;
        }

        private void GiveItem(InventoryItem.ITEM_TYPE type) => Inventory.AddItem((int)type, 1);
    }
}
