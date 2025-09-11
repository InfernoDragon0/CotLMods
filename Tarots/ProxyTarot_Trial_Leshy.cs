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
            return "<color=\"yellow\">Worms!</color> Yay!";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "Each <color=\"purple\">Active Augment</color>, whenever an enemy takes non lethal damage, there is a <color=\"yellow\">5% chance an Amdusia kin will spawn</color>.\r\nEach <color=\"red\">Active Trial</color>, additional <color=\"yellow\">Barbatos kins will appear in combat room</color>.";
        }

        public override string Skin => "Trinkets/CoopGoodTiming";
    }
}
