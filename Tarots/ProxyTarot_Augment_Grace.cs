using COTL_API.CustomTarotCard;

namespace CotLMiniMods.Tarots
{
    public class ProxyTarot_Augment_Grace : CustomTarotCard
    {
        public override string InternalName => "ProxyTarot_Augment_Grace";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"purple\">Augment: Grace</color>";
        }

        public override string LocalisedLore()
        {
            return "Augment of Grace";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "When an enemy dies, all other enemies <color=\"yellow\"> double their current and max hp</color>.";
        }

        public override string Skin => "Trinkets/NoCorruption";
    }
}
