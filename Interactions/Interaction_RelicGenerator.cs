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
    internal class Interaction_RelicGenerator : Interaction
    {
        public Structure Structure;
        private bool Activated;
        public StructuresData StructureInfo => this.Structure.Structure_Info;
        public Structures_RelicGenerator cCauldron => this.Structure.Brain as Structures_RelicGenerator;

        public int CurrentSuccesses = 0;
        public int maxSuccesses = 20;

        public UICookingMinigameOverlayController _uiCookingMinigameOverlayController;

        public override void GetLabel()
        {
            this.label = "10x Strange Material: Random Normal Relic";
            this.SecondaryLabel = "10x Strange Material: Random Blessed / Damned Relic";
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Plugin.Log.LogInfo("I am enabled Relic Generator");
            Structure = GetComponentInParent<Transform>().GetComponent<Structure>();

        }
        private void Start()
        {
            this.HasSecondaryInteraction = true;
        }

        public override void OnInteract(StateMachine state)
        {
            if (this.Activated) return;

            if (Inventory.GetItemQuantity(Plugin.StrangeMaterialItem) >= 10)
            {
                this.Activated = true;
                var randomRelic = EquipmentManager.GetRandomRelicData(true, RelicSubType.Any);
                PlayerFarming.Instance.playerRelic.EquipRelic(randomRelic, true, false);
                Inventory.ChangeItemQuantity(Plugin.StrangeMaterialItem, -10);

                //generate a random necklace
                this.Activated = false;
            }
            
        }

        public override void OnSecondaryInteract(StateMachine state)
        {
            if (this.Activated) return;
            if (Inventory.GetItemQuantity(Plugin.StrangeMaterialItem) >= 10)
            {
                this.Activated = true;
                var randomInt = UnityEngine.Random.Range(0, 2);
                var randomRelic = EquipmentManager.GetRandomRelicData(true, (RelicSubType)Math.Min(2, 1+randomInt));
                PlayerFarming.Instance.playerRelic.EquipRelic(randomRelic, true, false);
                Inventory.ChangeItemQuantity(Plugin.StrangeMaterialItem, -10);

                //generate a random necklace
                this.Activated = false;
            }
        }
    }
}
