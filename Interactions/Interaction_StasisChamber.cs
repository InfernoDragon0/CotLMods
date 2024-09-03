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
using CotLMiniMods.Structures;

namespace CotLMiniMods.Interactions
{
    internal class Interaction_StasisChamber : Interaction
    {
        public Structure Structure;
        private bool Activating = false;
        private GameObject Player;
        private float Delay = 60f;
        public float DistanceToTriggerDeposits = 5f;

        public StructuresData StructureInfo => this.Structure.Structure_Info;
        public Structures_StasisChamber StasisChamber => this.Structure.Brain as Structures_StasisChamber;
        public override void GetLabel()
        {
            if (this.Structure != null && this.StasisChamber != null)
            {
                if (this.StasisChamber.timeFrozen)
                {
                    this.label = "Unfreeze Time: Current Energy x" + this.StasisChamber.EnergyCurrent;

                }
                else
                {
                    this.label = "50 Energy to Freeze Time: Current Energy x" + this.StasisChamber.EnergyCurrent;
                }

            }
            else
                this.label = "structure was null";
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Plugin.Log.LogInfo("I am enabled Stasis Chamber Interaction");
            Structure = GetComponentInParent<Transform>().GetComponent<Structure>();
        }

        public override void OnInteract(StateMachine state)
        {
            if (this.Activating) return;
            base.OnInteract(state);
            this.Activating = true;
            if (this.StasisChamber.timeFrozen)
            {
                this.StasisChamber.timeFrozen = false;
                SimulationManager.UnPause();
                NotificationCentreScreen.Play("Simulation is now unfrozen. Back to `reality`.");
                this.Activating = false;
                return;
            }
            
            if (this.StasisChamber.EnergyCurrent < 50)
            {
                this.Activating = false;
                return;
            }

            this.StasisChamber.timeFrozen = true;
            this.StasisChamber.RemoveEnergy(50);
            NotificationCentreScreen.Play("Simulation is now frozen.");
            SimulationManager.Pause();
            this.Activating = false;


        }
        public override void Update()
        {
            base.Update();
            this.GetLabel();

            if (this.StasisChamber == null) return;

            if (this.StasisChamber.timeFrozen)
            {
                if ((double)(this.Delay -= Time.deltaTime) >= 0.0)
                    return;
                
                this.StasisChamber.RemoveEnergy(10);
                NotificationCentreScreen.Play("Simulation consumes Strange Energy when not fighting. " + this.StasisChamber.EnergyCurrent + " Energy left.");

                if (this.StasisChamber.EnergyCurrent <= 0)
                {
                    NotificationCentreScreen.Play("Warning: Stasis Chamber has no energy. Unfreezing time.");
                    this.StasisChamber.timeFrozen = false;
                    SimulationManager.UnPause();

                }

                if (this.StasisChamber.EnergyCurrent < 100)
                {
                    NotificationCentreScreen.Play("Warning: Stasis Chamber low on energy. " + this.StasisChamber.EnergyCurrent + " Energy left.");
                }

                this.Delay = 60f;

            }
        }
    }
}
