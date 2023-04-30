using COTL_API.CustomTarotCard;
using System;
using System.Collections.Generic;
using System.Text;

namespace CotLMiniMods.Tarots
{
    public class Tarot_StrangeExtraction : CustomTarotCard
    {
        public override string InternalName => "STRANGE_EXTRACTION";

        public override string LocalisedName(int upgradeIndex)
        {
            return "<color=\"red\">Strange Extraction</color>";
        }

        public override string LocalisedLore()
        {
            return "Our enemies.. <color=\"yellow\">they possess it..</color>";
        }

        public override string LocalisedDescription(int upgradeIndex)
        {
            return "When an enemy dies, they are converted into <color=\"red\">Strange Material</color>.";
        }

        public override string Skin => "Trinkets/Sun";

        public override InventoryItem GetItemToDrop(TarotCards.TarotCard card)
        {
            return new InventoryItem(Plugin.StrangeMaterialItem);
        } 
    }
}
