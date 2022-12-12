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
    internal class Interaction_StrangeGenerator : Interaction
    {
        private bool Activated;
        public Structure Structure;
        public StructuresData StructureInfo => this.Structure.Structure_Info;
        public Structures_StrangeGenerator StrangeGenerator => this.Structure.Brain as Structures_StrangeGenerator;
        public override void GetLabel()
        {
            if (StrangeGenerator != null)
            {
                this.label = "10x Strange Material, 100x Gold: 1000 Strange Energy. Current Energy: " + this.StrangeGenerator.EnergyCurrent;
            }
            else
                this.label = "structure was null";
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Plugin.Log.LogInfo("I am enabled Strange Generator");
            
        }

        public override void OnInteract(StateMachine state)
        {
            if (this.Activated) return;

            if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD) >= 100 && Inventory.GetItemQuantity(Plugin.StrangeMaterialItem) >= 10)
            {
                this.Activated = true;
                Inventory.ChangeItemQuantity(Plugin.StrangeMaterialItem, -10);
                Inventory.ChangeItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD, -100);
                ResourceCustomTarget.Create(PlayerFarming.Instance.gameObject, this.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
                ResourceCustomTarget.Create(PlayerFarming.Instance.gameObject, this.transform.position, Plugin.StrangeMaterialItem, null);

                StrangeGenerator.AddEnergy(1000);

                this.Activated = false;
            }
            
        }

        public override void Update()
        {
            this.GetLabel();
        }

    }
}
