using COTL_API.CustomTarotCard;

namespace CotLMiniMods.Tarots
{
    public class ProxyTarot_Augment_Bloodpact : CustomTarotCard
    {
        public override string InternalName => "ProxyTarot_Augment_Bloodpact";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"purple\">Augment: Bloodpact</color>";
        }

        public override string LocalisedLore()
        {
            return "Augment of Bloodpact";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "When an enemy dies, all enemies <color=\"yellow\">gain 10% damage</color>.";
        }

        public override string Skin => "Trinkets/NoCorruption";
    }
}
