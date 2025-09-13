using COTL_API.CustomTarotCard;

namespace CotLMiniMods.Tarots
{
    public class ProxyTarot_Augment_Persistence : CustomTarotCard
    {
        public override string InternalName => "ProxyTarot_Augment_Persistence";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"purple\">Augment: Persistence</color>";
        }

        public override string LocalisedLore()
        {
            return "Augment of Persistence";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "Enemies <color=\"yellow\">heal 5% of their health every 3 second</color>. Bosses <color=\"yellow\">heal 1% of their health per second</color>.";
        }

        public override string Skin => "Trinkets/NoCorruption";
    }
}
