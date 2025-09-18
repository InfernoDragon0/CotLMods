using COTL_API.CustomTarotCard;

namespace CotLMiniMods.Tarots
{
    public class ProxyTarot_Augment_Bombardment : CustomTarotCard
    {
        public override string InternalName => "ProxyTarot_Augment_Bombardment";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"purple\">Augment: Bombardment</color>";
        }

        public override string LocalisedLore()
        {
            return "Augment of Bombardment";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "Each time you attack, <color=\"yellow\">2 bombs spawn around you</color>.";
        }

        public override string Skin => "Trinkets/NoCorruption";
    }
}
