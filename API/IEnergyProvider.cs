using System;
using System.Collections.Generic;
using System.Text;

namespace CotLMiniMods.API
{
    public interface IEnergyProvider
    {
        int EnergyCurrent { get; set; }
        int EnergyMax { get; set; }
        int EnergyRegenRate { get; set; }

        bool CanAdd { get; } //can add energy into this machine? usually false for generators, true for machines
        bool CanRemove { get; } //can take out energy from this machine? usually true for generators, false for machines

        bool WorksAtNight { get; }
        bool WorksAtDay { get; }
        int AddEnergy(int amount);
        int RemoveEnergy(int amount);

        
    }
}
