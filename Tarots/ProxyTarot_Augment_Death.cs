using COTL_API.CustomTarotCard;

namespace CotLMiniMods.Tarots
{
    public class ProxyTarot_Augment_Exhaustion : CustomTarotCard
    {
        public override string InternalName => "ProxyTarot_Augment_Exhaustion";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"purple\">Augment: Exhaustion</color>";
        }

        public override string LocalisedLore()
        {
            return "Augment of Exhaustion";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "Each time you dodge, you lose <color=\"yellow\">0.1 movement speed</color>. Minimum 1.0 movement speed.";
        }

        public override string Skin => "Trinkets/NoCorruption";
    }
}
