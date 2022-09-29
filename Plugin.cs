using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using COTL_API.CustomInventory;
using CotLTemplateMod.Items;
using System.IO;
using static InventoryItem;
using COTL_API.CustomFollowerCommand;
using CotLTemplateMod.CustomFollowerCommands;
using BepInEx.Configuration;

namespace CotLTemplateMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("io.github.xhayper.COTL_API")]
    [HarmonyPatch]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "InfernoDragon0.cotl.CotLChef";
        public const string PluginName = "CotLChef";
        public const string PluginVer = "1.0.3";

        internal static ManualLogSource Log;
        internal readonly static Harmony Harmony = new(PluginGuid);

        internal static string PluginPath;


        internal static StructureBrain.TYPES FoodStructure;
        internal static WaiterCommand waiterTask;

        //configs
        internal static ConfigEntry<bool> biggerShrine;
        internal static ConfigEntry<bool> oneClickShrineCollection;
        internal static ConfigEntry<bool> skipSplash;
        internal static ConfigEntry<bool> kitchenDiscount;
        internal static ConfigEntry<bool> playerEatMore;

        internal static ConfigEntry<bool> deathCatRepeat;
        internal static ConfigEntry<bool> shrimpChefRepeat;

        internal static ConfigEntry<bool> chefJob;
        internal static ConfigEntry<bool> waiterJob; //FollowerTask_ResourceStation: Follower Location update + State Animation (TaskTick)

        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginName}!");
            Plugin.Log = base.Logger;

            PluginPath = Path.GetDirectoryName(Info.Location);

            biggerShrine = Config.Bind("", "biggerShrine", true, "Increases max capacity of shrine");
            oneClickShrineCollection = Config.Bind("", "oneClickShrineCollection", true, "Instantly collect all souls in one click from shrine");
            skipSplash = Config.Bind("", "skipSplash", true, "Skips the splash screen");
            kitchenDiscount = Config.Bind("", "kitchenDiscount", true, "Enables a 50% food discount on Kitchen 1");
            playerEatMore = Config.Bind("", "playerEatMore", true, "Allows you to eat as many times as you want per day.");

            deathCatRepeat = Config.Bind("", "deathCatRepeat", false, "Allows you to re-challenge The one who waits as many times as you want.");
            shrimpChefRepeat = Config.Bind("", "shrimpChefRepeat", false, "Allows you to re-challenge Chef Rakshasa as many times as you want.");

            chefJob = Config.Bind("", "chefJob", true, "Allows followers to take on the chef role.");
            waiterJob = Config.Bind("", "waiterJob", true, "Allows followers to take on the waiter role. If you enable this, followers will not walk towards food, but wait for someone to serve.");

            if (waiterJob.Value)
            {
                waiterTask = new WaiterCommand();
                CustomFollowerCommandManager.Add(waiterTask);
            }
            
        }

        private void OnEnable()
        {
            Harmony.PatchAll();
            Logger.LogInfo($"Loaded {PluginName}!");
        }

        private void OnDisable()
        {
            Harmony.UnpatchSelf();
            Logger.LogInfo($"Unloaded {PluginName}!");
        }
    }
}