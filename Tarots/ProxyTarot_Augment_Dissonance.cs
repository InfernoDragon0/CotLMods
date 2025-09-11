using COTL_API.CustomTarotCard;

namespace CotLMiniMods.Tarots
{
    public class ProxyTarot_Augment_Dissonance : CustomTarotCard
    {
        public override string InternalName => "ProxyTarot_Augment_Dissonance";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"purple\">Augment: Dissonance</color>";
        }

        public override string LocalisedLore()
        {
            return "Augment of Dissonance";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "Enemy spawns are <color=\"yellow\">randomized</color>.";
        }

        public override string Skin => "Trinkets/NoCorruption";
    }
}
