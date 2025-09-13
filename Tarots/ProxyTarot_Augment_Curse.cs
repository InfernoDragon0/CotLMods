using COTL_API.CustomTarotCard;

namespace CotLMiniMods.Tarots
{
    public class ProxyTarot_Augment_Curse : CustomTarotCard
    {
        public override string InternalName => "ProxyTarot_Augment_Curse";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"purple\">Augment: Curse</color>";
        }

        public override string LocalisedLore()
        {
            return "Augment of Curse";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "Each time you attack, you lose <color=\"yellow\">5% Curse Charge</color>.";
        }

        public override string Skin => "Trinkets/NoCorruption";
    }
}
