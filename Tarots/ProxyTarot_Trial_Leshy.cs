using COTL_API.CustomTarotCard;

namespace CotLMiniMods.Tarots
{
    public class ProxyTarot_Trial_Leshy : CustomTarotCard
    {
        public override string InternalName => "ProxyTarot_Trial_Leshy";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"red\">Trial of Leshy</color>";
        }

        public override string LocalisedLore()
        {
            return "PURE... <color=\"yellow\">CHAOS!</color>";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "Each <color=\"purple\">Active Augment</color>, whenever an enemy takes non lethal damage, there is a <color=\"yellow\">5% chance they duplicate</color>.\r\nEach <color=\"red\">Active Trial</color>, an additional <color=\"yellow\">each mob spawns</color>.";
        }

        public override string Skin => "Trinkets/CoopGoodTiming";
    }
}
