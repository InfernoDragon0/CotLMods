using COTL_API.CustomStructures;
using COTL_API.Helpers;
using CotLMiniMods.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CotLMiniMods.Structures
{
    internal class CustomEnergyStructure : CustomStructure, IEnergyProvider
    {
        public override string InternalName => "Custom_Energy_Structure";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/endlesspit.png"));
        public override int BuildDurationMinutes => 30;

        public int EnergyCurrent { get => currentEnergy; set => this.currentEnergy = value; }
        public virtual int EnergyMax { get => maxEnergy; set => this.maxEnergy = value; }
        public virtual int EnergyRegenRate { get => 1; set => this.regenRate = value; }

        public virtual bool CanAdd => false;
        public virtual bool CanRemove => true;
        public virtual bool WorksAtNight => true;
        public virtual bool WorksAtDay => true;

        public int currentEnergy = 0;
        public int maxEnergy = 1000;
        public int regenRate = 0;

        public int AddEnergy(int amount)
        {
            int finalChange = 0;
            if (amount < 0) return finalChange;

            if (this.EnergyCurrent + amount >= this.EnergyMax)
            {
                finalChange = this.EnergyMax - this.EnergyCurrent;
                this.EnergyCurrent = this.EnergyMax;
            }
            else
            {
                this.EnergyCurrent += amount;
                finalChange = amount;
            }

            return finalChange;
        }

        public int RemoveEnergy(int amount)
        {
            int finalChange = 0;
            if (amount < 0) return finalChange;

            if (this.EnergyCurrent - amount <= 0)
            {
                finalChange = this.EnergyCurrent;
                this.EnergyCurrent = 0;
            }
            else
            {
                finalChange = amount;
                this.EnergyCurrent -= amount;
            }
            return finalChange;
        }
    }
}
