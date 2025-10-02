using COTL_API.CustomTarotCard;

namespace CotLMiniMods.Tarots
{
    public class ProxyTarot_Trial_Kallamar : CustomTarotCard
    {
        public override string InternalName => "ProxyTarot_Trial_Kallamar";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"red\">Trial of Kallamar</color>";
        }

        public override string LocalisedLore()
        {
            return "Step <color=\"yellow\">Carefully..</color>";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "Each <color=\"purple\">Active Augment</color>, this effect speeds up by 1 second. Every 15 seconds, <color=\"yellow\">a pool of poison is spawned on your location</color>. \r\nEach <color=\"red\">Active Trial</color>, this effect speeds up by 1 second. Every 10 seconds, <color=\"yellow\">all enemies drop a pool of poison</color>. ";
        }

        public override string Skin => "Trinkets/CoopGoodTiming";
    }
}
