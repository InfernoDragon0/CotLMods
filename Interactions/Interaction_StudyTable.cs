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

namespace CotLMiniMods.Interactions
{
    internal class Interaction_StudyTable : Interaction
    {
        public Structure Structure;
        private bool Activated;
        public override void GetLabel()
        {
            this.label = "Invest x1500 Gold to generate Strange Material";
            if (this.Structure != null)
            {
                this.label = "x1500 Gold: Increase Strange Material generation (Currently x" + this.Structure.Inventory.Count + ")";
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Plugin.Log.LogInfo("I am enabled Study Table");
            Structure = GetComponentInParent<Transform>().GetComponent<Structure>();
        }

        public override void OnInteract(StateMachine state)
        {
            if (this.Activated) return;

            if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD) >= 1500)
            {
                this.Activated = true;
                Inventory.ChangeItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD, -1500);
                ResourceCustomTarget.Create(this.gameObject, PlayerFarming.Instance.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
                this.Structure.Inventory.Add(new InventoryItem(Plugin.StrangeMaterialItem));
                this.Activated = false;
            }
            
        }


    }
}
