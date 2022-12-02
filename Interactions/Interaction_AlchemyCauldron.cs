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
    internal class Interaction_AlchemyCauldron : Interaction
    {
        private bool Activated;
        public override void GetLabel()
        {
            this.label = "5x Strange Material, 100x Gold: Random Necklace";
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Plugin.Log.LogInfo("I am enabled Alchemy Cauldron");
        }

        public override void OnInteract(StateMachine state)
        {
            if (this.Activated) return;

            if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD) >= 100 && Inventory.GetItemQuantity(Plugin.StrangeMaterialItem) >= 5)
            {
                this.Activated = true;
                Inventory.ChangeItemQuantity(Plugin.StrangeMaterialItem, -5);
                Inventory.ChangeItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD, -100);
                ResourceCustomTarget.Create(PlayerFarming.Instance.gameObject, this.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);
                ResourceCustomTarget.Create(PlayerFarming.Instance.gameObject, this.transform.position, Plugin.StrangeMaterialItem, null);


                //generate a random necklace
                int rand = Random.Range(45, 50);
                InventoryItem.Spawn((InventoryItem.ITEM_TYPE)rand, 1, this.Position);

                GameManager.GetInstance().OnConversationNew();
                this.Activated = false;
            }
            
        }


    }
}
