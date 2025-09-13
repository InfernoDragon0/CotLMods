using COTL_API.CustomTarotCard;

namespace CotLMiniMods.Tarots
{
    public class ProxyTarot_Trial_Heket : CustomTarotCard
    {
        public override string InternalName => "ProxyTarot_Trial_Heket";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"red\">Trial of Heket</color>";
        }

        public override string LocalisedLore()
        {
            return "We <color=\"yellow\">Hunger...</color>";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "Each <color=\"purple\">Active Augment</color>, whenever you take damage, <color=\"yellow\">you lose 10% Curse Charge</color>.\r\nEach <color=\"red\">Active Trial</color>, whenever you take damage, you have a <color=\"yellow\">10% chance of losing a tarot card</color>.";
        }

        public override string Skin => "Trinkets/CoopGoodTiming";
    }
}
