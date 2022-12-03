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
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Enable Chef Job", Plugin.chefJob, null);
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Enable Waiter Job", Plugin.waiterJob, null);
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Enable Fisher Job", Plugin.fisherJob, null);
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Enable Reaper Job", Plugin.reaperJob, null);
            
            //Bools that add custom stuff
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Enable Custom Food", Plugin.customFood, null);
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Enable Custom Rituals", Plugin.customRituals, null);
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Enable Custom Structures", Plugin.customStructures, null);

            //Bools for fight repeat
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Death Cat Repeat", Plugin.deathCatRepeat, null);
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Shrimp Chef Repeat", Plugin.shrimpChefRepeat, null);

            //Bools for others tuff
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Player Eat unlimited", Plugin.playerEatMore, null);
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Kitchen Discount", Plugin.kitchenDiscount, null);
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Skip Splash", Plugin.skipSplash, null);
            CustomSettingsManager.AddBepInExConfig("CotLMiniMods","One Click Shrine Collection", Plugin.oneClickShrineCollection, null);


            //int values
            //CustomSettingsManager.AddBepInExConfig("CotLMiniMods","Set Shrine Size", Plugin.shrineSize, null);
            
        }
    }
}
