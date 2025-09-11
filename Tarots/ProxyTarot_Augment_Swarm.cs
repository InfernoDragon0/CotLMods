using COTL_API.CustomTarotCard;

namespace CotLMiniMods.Tarots
{
    public class ProxyTarot_Augment_Swarm : CustomTarotCard
    {
        public override string InternalName => "ProxyTarot_Augment_Swarm";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"purple\">Augment: Swarm</color>";
        }

        public override string LocalisedLore()
        {
            return "Augment of Swarm";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "All non-boss enemy spawns are <color=\"yellow\">doubled</color>.";
        }

        public override string Skin => "Trinkets/NoCorruption";
    }
}
