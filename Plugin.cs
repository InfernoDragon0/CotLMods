using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using COTL_API.CustomFollowerCommand;
using CotLTemplateMod.CustomFollowerCommands;
using BepInEx.Configuration;
using CotLMiniMods.Meals;
using COTL_API.CustomStructures;
using CotLMiniMods.Structures;
using CotLMiniMods.Patches.Rituals;
using CotLMiniMods.Rituals;
using CotLMiniMods.Structures.Mines;
using CotLMiniMods.Structures.Proxies;
using CotLMiniMods.Structures.Productivity;
using CotLMiniMods.Items;
using COTL_API.CustomInventory;
using CotLMiniMods.CustomFollowerCommands;
using COTL_API.CustomSettings;

namespace CotLMiniMods
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("io.github.xhayper.COTL_API")]
    [HarmonyPatch]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "InfernoDragon0.cotl.CotLChef";
        public const string PluginName = "CotLChef";
        public const string PluginVer = "1.1.4";

        internal static ManualLogSource Log;
        internal readonly static Harmony Harmony = new(PluginGuid);

        internal static string PluginPath;

        internal static WaiterCommand waiterTask;
        internal static FisherCommand fisherTask;
        internal static DivorceCommand DivorceCommand;
        internal static FlipCoinCommand FlipCoinCommand;
        internal static KnucklebonesCommand KnucklebonesCommand;
        internal static BoneMineCommand BoneMineCommand;
        internal static SilkMineCommand SilkMineCommand;
        internal static CrystalMineCommand CrystalMineCommand;
        
        internal static HRManagementStructure HRManagementStructure;
        internal static CrystalMineStructure CrystalMineStructure;
        internal static BoneMineStructure BoneMineStructure;
        internal static SilkMineStructure SilkMineStructure;
        internal static WishingWellStructure WishingWellStructure;
        internal static WaiterDeskStructure WaiterDeskStructure;
        internal static ChefDeskStructure ChefDeskStructure;
        internal static Structures_LuckyNarinder Structures_LuckyNarinder;
        internal static Structures_StudyTable Structures_StudyTable;
        internal static Structures_Telescope Structures_Telescope;
        internal static Structures_AlchemyCauldron Structures_AlchemyCauldron;
        internal static Structures_EndlessPit Structures_EndlessPit;

        internal static InventoryItem.ITEM_TYPE StrangeMaterialItem;
        internal static InventoryItem.ITEM_TYPE StrangeEnergyItem;

        //configs
        internal static ConfigEntry<bool> biggerShrine;
        internal static ConfigEntry<int> shrineSize;
        
        internal static ConfigEntry<bool> oneClickShrineCollection;
        internal static ConfigEntry<bool> skipSplash;
        internal static ConfigEntry<bool> kitchenDiscount;
        internal static ConfigEntry<bool> playerEatMore;

        internal static ConfigEntry<bool> deathCatRepeat;
        internal static ConfigEntry<bool> shrimpChefRepeat;

        internal static ConfigEntry<bool> chefJob;
        internal static ConfigEntry<bool> fisherJob;
        internal static ConfigEntry<bool> waiterJob;
        internal static ConfigEntry<bool> reaperJob;

        internal static ConfigEntry<bool> customFood;
        internal static ConfigEntry<bool> customStructures;
        internal static ConfigEntry<bool> customRituals;

        private void Awake()
        {
            Logger.LogInfo($"Loaded {PluginName}!");
            Plugin.Log = base.Logger;

            PluginPath = Path.GetDirectoryName(Info.Location);

            biggerShrine = Config.Bind("", "biggerShrine", true, "Increases max capacity of shrine");
            shrineSize = Config.Bind("", "shrineSize", 2000, "Max capacity of shrine");
            oneClickShrineCollection = Config.Bind("", "oneClickShrineCollection", true, "Instantly collect all souls in one click from shrine");
            skipSplash = Config.Bind("", "skipSplash", true, "Skips the splash screen");
            kitchenDiscount = Config.Bind("", "kitchenDiscount", true, "Enables a 50% food discount on Kitchen 1");
            playerEatMore = Config.Bind("", "playerEatMore", true, "Allows you to eat as many times as you want per day.");

            deathCatRepeat = Config.Bind("", "deathCatRepeat", false, "Allows you to re-challenge The one who waits as many times as you want.");
            shrimpChefRepeat = Config.Bind("", "shrimpChefRepeat", false, "Allows you to re-challenge Chef Rakshasa as many times as you want.");

            chefJob = Config.Bind("", "chefJob", true, "Allows followers to take on the chef role.");
            fisherJob = Config.Bind("", "fisherJob", true, "Allows you to command followers to fish (different loot table).");
            waiterJob = Config.Bind("", "waiterJob", true, "Allows followers to take on the waiter role. If you enable this, followers will not walk towards food, but wait for someone to serve.");
            reaperJob = Config.Bind("", "reaperJob", true, "Allows followers to bury other followers in an Endless Pit.");
            
            customFood = Config.Bind("", "customFood", false, "Adds custom food (partial implementation, not ready yet).");
            customStructures = Config.Bind("", "customStructures", true, "Adds Custom Structures.");
            customRituals = Config.Bind("", "customRituals", true, "Adds Custom Rituals.");

            ConfigListener.AddConfigEntries();
            

            if (waiterJob.Value)
            {
                waiterTask = new WaiterCommand();
                CustomFollowerCommandManager.Add(waiterTask);
            }

            if (fisherJob.Value)
            {
                fisherTask = new FisherCommand();
                CustomFollowerCommandManager.Add(fisherTask);
            }

            if (customFood.Value)
            {
                CustomMealManager.AddAll();
            }

            if (customStructures.Value)
            {
                HRManagementStructure = new HRManagementStructure();
                CustomStructureManager.Add(HRManagementStructure);

                CrystalMineStructure = new CrystalMineStructure();
                CustomStructureManager.Add(CrystalMineStructure);

                WishingWellStructure = new WishingWellStructure();
                CustomStructureManager.Add(WishingWellStructure);

                BoneMineStructure = new BoneMineStructure();
                CustomStructureManager.Add(BoneMineStructure);

                SilkMineStructure = new SilkMineStructure();
                CustomStructureManager.Add(SilkMineStructure);

                WaiterDeskStructure = new WaiterDeskStructure();
                CustomStructureManager.Add(WaiterDeskStructure);

                ChefDeskStructure = new ChefDeskStructure();
                CustomStructureManager.Add(ChefDeskStructure);

                Structures_LuckyNarinder = new Structures_LuckyNarinder();
                CustomStructureManager.Add(Structures_LuckyNarinder);

                Structures_StudyTable = new Structures_StudyTable();
                CustomStructureManager.Add(Structures_StudyTable);

                Structures_Telescope = new Structures_Telescope();
                CustomStructureManager.Add(Structures_Telescope);
                
                Structures_AlchemyCauldron = new Structures_AlchemyCauldron();
                CustomStructureManager.Add(Structures_AlchemyCauldron);

                Structures_EndlessPit = new Structures_EndlessPit();
                CustomStructureManager.Add(Structures_EndlessPit);

                CustomStructureManager.Add(new Structures_StrangeGenerator());
                CustomStructureManager.Add(new Structures_EnergyController());
                CustomStructureManager.Add(new Structures_StasisChamber());
                CustomStructureManager.Add(new Structures_AIOQuarry());
                CustomStructureManager.Add(new Structures_AIOFarmer());

                BoneMineCommand = new BoneMineCommand();
                CustomFollowerCommandManager.Add(BoneMineCommand);

                SilkMineCommand = new SilkMineCommand();
                CustomFollowerCommandManager.Add(SilkMineCommand);

                CrystalMineCommand = new CrystalMineCommand();
                CustomFollowerCommandManager.Add(CrystalMineCommand);

                

                StrangeMaterialItem = CustomItemManager.Add(new StrangeMaterialItem());
                StrangeEnergyItem = CustomItemManager.Add(new StrangeEnergyItem());

            }   

            

            DivorceCommand = new DivorceCommand();
            CustomFollowerCommandManager.Add(DivorceCommand);

            FlipCoinCommand = new FlipCoinCommand();
            CustomFollowerCommandManager.Add(FlipCoinCommand);

            KnucklebonesCommand = new KnucklebonesCommand();
            CustomFollowerCommandManager.Add(KnucklebonesCommand);

            if (customRituals.Value)
            {
                CustomRitualManager.Add(new DistributionRitual());
                CustomRitualManager.Add(new RitualFrenzyRitual());
                CustomRitualManager.Add(new FusionRitual());
                CustomRitualManager.Add(new YouthRitual());
                CustomRitualManager.Add(new ReversalRitual());
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