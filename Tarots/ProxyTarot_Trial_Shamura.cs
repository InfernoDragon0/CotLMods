using COTL_API.CustomTarotCard;

namespace CotLMiniMods.Tarots
{
    public class ProxyTarot_Trial_Shamura : CustomTarotCard
    {
        public override string InternalName => "ProxyTarot_Trial_Shamura";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"red\">Trial of Shamura</color>";
        }

        public override string LocalisedLore()
        {
            return "<color=\"yellow\">Peace</color> was never an option.";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "Each <color=\"purple\">Active Augment</color>, whenever enemies take damage, their <color=\"yellow\">damage increases by 5%</color>.\r\nEach <color=\"red\">Active Trial</color>, whenever you take damage, <color=\"yellow\">all enemies will heal for 10% of their max health</color>.";
        }

        public override string Skin => "Trinkets/CoopGoodTiming";
    }
}
