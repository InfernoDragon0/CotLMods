using COTL_API.CustomTarotCard;

namespace CotLMiniMods.Tarots
{
    public class ProxyTarot_Augment_Resistance : CustomTarotCard
    {
        public override string InternalName => "ProxyTarot_Augment_Resistance";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"purple\">Augment: Resistance</color>";
        }

        public override string LocalisedLore()
        {
            return "Augment of Resistance";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "Enemies have a <color=\"yellow\">25% chance of not taking damage from hits</color>.";
        }

        public override string Skin => "Trinkets/NoCorruption";
    }
}
