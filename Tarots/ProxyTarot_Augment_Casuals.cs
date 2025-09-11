using COTL_API.CustomTarotCard;

namespace CotLMiniMods.Tarots
{
    public class ProxyTarot_Augment_Casuals : CustomTarotCard
    {
        public override string InternalName => "ProxyTarot_Augment_Casuals";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"purple\">Augment: Casuals</color>";
        }

        public override string LocalisedLore()
        {
            return "Augment of Casuals";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "<color=\"yellow\">1 Juggernaut</color> spawn per combat room.";
        }

        public override string Skin => "Trinkets/NoCorruption";
    }
}
