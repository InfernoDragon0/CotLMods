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
using Lamb.UI;
using COTL_API.CustomTarotCard;
using CotLMiniMods.Tarots;
using System;
using COTL_API.CustomLocalization;
using System.Collections.Generic;

namespace CotLMiniMods
{
    [BepInPlugin(PluginGuid, PluginName, PluginVer)]
    [BepInDependency("io.github.xhayper.COTL_API")]
    [HarmonyPatch]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "InfernoDragon0.cotl.CotLChef";
        public const string PluginName = "CotLMiniMods";
        public const string PluginVer = "1.2.3";

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
        internal static FishingStructure FishingStructure;
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
        internal static Structures_RelicGenerator Structures_RelicGenerator;

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
        internal static ConfigEntry<bool> customTarots;
        
        internal static ConfigEntry<bool> telescopeGivesQuest;
        internal static ConfigEntry<bool> relicNoReset;

        internal static ConfigEntry<bool> NoExhaustMating;
        internal static ConfigEntry<string> localizationConfig;

        internal static bool SinnedToday = false;
        
        public static List<TarotCards.TarotCard> wishedCards = [];
        public static RelicType relicData = RelicType.None;

        public static Dictionary<TarotCards.Card, CustomTarotCard> proxyTrials = [];
        public static Dictionary<TarotCards.Card, CustomTarotCard> proxyAugments = [];

        public static List<TarotCards.Card> proxyTrialsEnabled = [];
        public static List<TarotCards.Card> proxyAugmentsEnabled = [];


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
            customTarots = Config.Bind("", "customTarots", true, "Adds Custom Tarots.");
            telescopeGivesQuest = Config.Bind("", "telescopeGivesQuest", true, "Set to true if the telescope should give quests in the morning. False will provide Strange Material at a lower rate in the day.");
            relicNoReset = Config.Bind("", "relicNoReset", true, "Set to true for the Relic Infuser to work, and getting to keep relics after runs.");
            NoExhaustMating = Config.Bind("", "NoExhaustMating", true, "Set to true to allow mating without exhausting the follower.");
            localizationConfig = Config.Bind("", "Localization", "English", "Set to your preferred language (you must have localization files for this mod)");


            ConfigListener.AddConfigEntries();
            TimeManager.OnNewDayStarted += new System.Action(this.OnNewDayStarted);
            CustomLocalizationManager.LoadLocalization("English", Path.Combine(Plugin.PluginPath, "Assets/Localization/" + localizationConfig.Value + ".language"));


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

                FishingStructure = new FishingStructure();
                CustomStructureManager.Add(FishingStructure);

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

                Structures_RelicGenerator = new Structures_RelicGenerator();
                CustomStructureManager.Add(Structures_RelicGenerator);

                CustomStructureManager.Add(new Structures_StrangeGenerator());
                CustomStructureManager.Add(new Structures_EnergyController());
                CustomStructureManager.Add(new Structures_StasisChamber());
                CustomStructureManager.Add(new Structures_AIOQuarry());
                CustomStructureManager.Add(new Structures_AIOFarmer());
                CustomStructureManager.Add(new Structures_GiftTree());
                CustomStructureManager.Add(new Structures_Boutique());

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

            CustomFollowerCommandManager.Add(new Command_ForIHaveSinned());
            if (customRituals.Value)
            {
                CustomRitualManager.Add(new DistributionRitual());
                CustomRitualManager.Add(new RitualFrenzyRitual());
                CustomRitualManager.Add(new FusionRitual());
                CustomRitualManager.Add(new YouthRitual());
                CustomRitualManager.Add(new ReversalRitual());
                //CustomRitualManager.Add(new SuperchargeRitual());
                CustomRitualManager.Add(new MistletoeRitual());
                CustomRitualManager.Add(new MassResurrectionRitual());
                CustomRitualManager.Add(new ExiledRitual());
                CustomRitualManager.Add(new MassacreRitual());
            }

            if (customTarots.Value)
            {
                Plugin.Log.LogInfo("Added tarots");
                CustomTarotCardManager.Add(new Tarot_StrangeExtraction());


                //trials
                var leshy = new ProxyTarot_Trial_Leshy();
                var kallamar = new ProxyTarot_Trial_Kallamar();
                var heket = new ProxyTarot_Trial_Heket();
                var shamura = new ProxyTarot_Trial_Shamura();
                var narinder = new ProxyTarot_Trial_Narinder();

                var leshycard = CustomTarotCardManager.Add(leshy);
                var kallamarcard = CustomTarotCardManager.Add(kallamar);
                var heketcard = CustomTarotCardManager.Add(heket);
                var shamuracard = CustomTarotCardManager.Add(shamura);
                var narindercard = CustomTarotCardManager.Add(narinder);

                proxyTrials.Add(leshycard, leshy);
                proxyTrials.Add(kallamarcard, kallamar);
                proxyTrials.Add(heketcard, heket);
                proxyTrials.Add(shamuracard, shamura);
                proxyTrials.Add(narindercard, narinder);

                //augments
                var casuals = new ProxyTarot_Augment_Casuals();
                var cultists = new ProxyTarot_Augment_Cultists();
                var death = new ProxyTarot_Augment_Death();
                var dissonance = new ProxyTarot_Augment_Dissonance();
                var Bloodpact = new ProxyTarot_Augment_Bloodpact();
                var healing = new ProxyTarot_Augment_Healing();
                var persistence = new ProxyTarot_Augment_Persistence();
                var swarm = new ProxyTarot_Augment_Swarm();
                var resistance = new ProxyTarot_Augment_Resistance();
                var explosion = new ProxyTarot_Augment_Explosion();

                var casualsCard = CustomTarotCardManager.Add(casuals);
                var cultistsCard = CustomTarotCardManager.Add(cultists);
                var deathCard = CustomTarotCardManager.Add(death);
                var dissonanceCard = CustomTarotCardManager.Add(dissonance);
                var BloodpactCard = CustomTarotCardManager.Add(Bloodpact);
                var healingCard = CustomTarotCardManager.Add(healing);
                var persistenceCard = CustomTarotCardManager.Add(persistence);
                var swarmCard = CustomTarotCardManager.Add(swarm);
                var resistanceCard = CustomTarotCardManager.Add(resistance);
                var explosionCard = CustomTarotCardManager.Add(explosion);

                proxyAugments.Add(casualsCard, casuals);
                proxyAugments.Add(cultistsCard, cultists);
                proxyAugments.Add(deathCard, death);
                proxyAugments.Add(dissonanceCard, dissonance);
                proxyAugments.Add(BloodpactCard, Bloodpact);
                proxyAugments.Add(healingCard, healing);
                proxyAugments.Add(persistenceCard, persistence);
                proxyAugments.Add(swarmCard, swarm);
                proxyAugments.Add(resistanceCard, resistance);
                proxyAugments.Add(explosionCard, explosion);

            }
        }

        private void OnNewDayStarted()
        {
            SinnedToday = false;
            Plugin.Log.LogInfo("You can now sin again.");
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