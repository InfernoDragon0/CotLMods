using BepInEx.Configuration;
using COTL_API.CustomSettings;
using CotLTemplateMod;
using System;
using System.Collections.Generic;
using System.Text;

namespace CotLMiniMods
{
    internal class ConfigListener
    {
        public static void AddConfigEntries()
        {
            //add these to the bepinex config manager
            //BOOLs for tasks
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Enable Chef Job", Plugin.chefJob);
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Enable Waiter Job", Plugin.waiterJob);
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Enable Fisher Job", Plugin.fisherJob);
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Enable Reaper Job", Plugin.reaperJob);
            
            //Bools that add custom stuff
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Enable Custom Food", Plugin.customFood);
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Enable Custom Rituals", Plugin.customRituals);
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Enable Custom Structures", Plugin.customStructures);

            //Bools for fight repeat
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Death Cat Repeat", Plugin.deathCatRepeat);
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Shrimp Chef Repeat", Plugin.shrimpChefRepeat);

            //Bools for others tuff
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Player Eat unlimited", Plugin.playerEatMore);
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Kitchen Discount", Plugin.kitchenDiscount);
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Skip Splash", Plugin.skipSplash);
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","One Click Shrine Collection", Plugin.oneClickShrineCollection);


            //int values
            //CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Set Shrine Size", Plugin.shrineSize, null);
            
        }
    }
}
