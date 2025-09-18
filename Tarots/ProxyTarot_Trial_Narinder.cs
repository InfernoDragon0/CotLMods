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
            return "TRYING TO OUTRUN.. <color=\"yellow\">DEATH?</color> Poor little Lamb.... HAHAHAHA";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "Each <color=\"purple\">Active Augment</color> will increase your <color=\"yellow\">dodge cooldown by 25%</color>.\r\nEach <color=\"red\">Active Trial</color> will grant enemies a <color=\"yellow\">10% chance of instantly killing you on hit</color>, and increases your <color=\"yellow\">Vulnerability to Damage by 1% for each enemy you have killed during the run</color>.";
        }

        public override string Skin => "Trinkets/CoopGoodTiming";
    }
}
