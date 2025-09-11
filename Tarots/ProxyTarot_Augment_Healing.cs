using COTL_API.CustomTarotCard;

namespace CotLMiniMods.Tarots
{
    public class ProxyTarot_Augment_Healing : CustomTarotCard
    {
        public override string InternalName => "ProxyTarot_Augment_Healing";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"purple\">Augment: Healing</color>";
        }

        public override string LocalisedLore()
        {
            return "Augment of Healing";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "<color=\"yellow\">2 Healers</color> spawn per combat room.";
        }

        public override string Skin => "Trinkets/NoCorruption";
    }
}
