using COTL_API.CustomTarotCard;

namespace CotLMiniMods.Tarots
{
    public class ProxyTarot_Augment_Death : CustomTarotCard
    {
        public override string InternalName => "ProxyTarot_Augment_Death";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"purple\">Augment: Death</color>";
        }

        public override string LocalisedLore()
        {
            return "Augment of Death";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "When an enemy hits you, they have a <color=\"yellow\">25% chance of spawning a poison puddle under them</color>.";
        }

        public override string Skin => "Trinkets/NoCorruption";
    }
}
