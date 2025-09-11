using COTL_API.CustomTarotCard;

namespace CotLMiniMods.Tarots
{
    public class ProxyTarot_Augment_Explosion : CustomTarotCard
    {
        public override string InternalName => "ProxyTarot_Augment_Explosion";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"purple\">Augment: Explosion</color>";
        }

        public override string LocalisedLore()
        {
            return "Augment of Explosion";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "Enemies <color=\"yellow\">explode</color> on death.";
        }

        public override string Skin => "Trinkets/NoCorruption";
    }
}
