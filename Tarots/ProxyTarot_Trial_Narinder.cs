using COTL_API.CustomTarotCard;

namespace CotLMiniMods.Tarots
{
    public class ProxyTarot_Trial_Narinder : CustomTarotCard
    {
        public override string InternalName => "ProxyTarot_Trial_Narinder";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"red\">Trial of Narinder</color>";
        }

        public override string LocalisedLore()
        {
            return "This is my.. <color=\"yellow\">Final test...</color> Little lamb....";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "Each <color=\"purple\">Active Augment</color> will increase all enemies' <color=\"yellow\">health by 50%</color>.\r\nEach <color=\"red\">Active Trial</color> will grant enemies a <color=\"yellow\">10% chance of instantly killing you on hit</color>, and increases the health and damage of the final boss by <color=\"yellow\">5% for each enemy you have killed during the run</color>, and whenever a non boss enemy dies, they have a <color=\"yellow\">5% chance of reviving if they are not the last enemy</color>.";
        }

        public override string Skin => "Trinkets/CoopGoodTiming";
    }
}
