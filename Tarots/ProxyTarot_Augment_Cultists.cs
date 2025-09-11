using COTL_API.CustomTarotCard;

namespace CotLMiniMods.Tarots
{
    public class ProxyTarot_Augment_Cultists : CustomTarotCard
    {
        public override string InternalName => "ProxyTarot_Augment_Cultists";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"purple\">Augment: Cultists</color>";
        }

        public override string LocalisedLore()
        {
            return "Augment of Cultists";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "<color=\"yellow\">5 random Cultists</color> spawn per combat room.";
        }

        public override string Skin => "Trinkets/NoCorruption";
    }
}
