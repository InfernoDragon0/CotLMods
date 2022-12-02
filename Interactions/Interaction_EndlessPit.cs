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
using CotLMiniMods.Structures;

namespace CotLMiniMods.Interactions
{
    internal class Interaction_EndlessPit : Interaction
    {
        private Structure Structure;
        private bool Activated;
        public override void GetLabel()
        {
            Structures_EndlessPit pit = this.Structure.Brain as Structures_EndlessPit;
            if (pit != null)
            { //todo: next update move to wireless batteries
                if (PlayerFarming.Instance.CarryingDeadFollowerID != -1)
                    this.label = "Bury Follower to generate Strange Energy.";
                else
                    this.label = "Current Status: [" + pit.Data.Inventory.Count + " Followers][" + pit.currentEnergy + " Strange Energy]";
            }

            else
            {
                this.label = "Pit not initialized yet.";

            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Plugin.Log.LogInfo("I am enabled Endless Pit");
            Structure = GetComponentInParent<Transform>().GetComponent<Structure>();
        }

        public override void OnInteract(StateMachine state)
        {
            if (this.Activated) return;

            if (PlayerFarming.Instance.CarryingDeadFollowerID != -1)
            {
                //check if lamb is holding a corpse, then add it in, doesnt matter the follower id, it is endless after all
                PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
                PlayerFarming.Instance.CarryingDeadFollowerID = -1;

                //add the follower into the inventory, using a proxy item
                this.Structure.Structure_Info.Inventory.Add(new InventoryItem(InventoryItem.ITEM_TYPE.BONE));
                
            }
            
        }


    }
}
