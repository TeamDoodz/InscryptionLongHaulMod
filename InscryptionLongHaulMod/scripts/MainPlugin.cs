using System;
using BepInEx;
using APIPlugin;
using UnityEngine;
using BepInEx.Configuration;
using DiskCardGame;
using System.Text.RegularExpressions;

namespace LongHaulMod {
	[BepInPlugin(GUID, Name, Version)]
	[BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency)]
	public class MainPlugin : BaseUnityPlugin {

		public struct ConfigOptions {
			public bool TraderFixEnabled;
			public string[] RareCardIgnorelist;
			public bool BossModuleEnabled;
			public bool Gunbots;
			public bool GunbotsTradeable;
			internal bool TradeRareCards;
			internal bool RareRequiresGP;
			internal int GunbotCost;
			internal int GunbotAttack;
			internal int GunbotHealth;
			internal bool ProspectorDontClearQueue;
		}

		private const string GUID = "io.github.TeamDoodz.LongHaulMod";
		private const string Name = "LongHaulMod";
		private const string Version = "1.0.0";

		public static ConfigOptions config;

		public static BepInEx.Logging.ManualLogSource logger;

		private TraderFixModule tf;
		private BossModule bm;

		private void Awake() {
			Logger.LogInfo($"{Name} has been awoken");
			logger = Logger;

			GetConfig();

			if (config.TraderFixEnabled) tf = new TraderFixModule(this);
			if(config.BossModuleEnabled) bm = new BossModule(this);

			if (config.BossModuleEnabled) bm.Awake();
		}

		private void Start() {
			Logger.LogInfo("Start called");

			if(config.TraderFixEnabled) tf.Start();
		}

		private void GetConfig(bool createIfNonexistent = true) {
			Logger.LogInfo($"Reading config...");

			config.TraderFixEnabled = Config.Bind("TraderFixModule", "TraderFixEnabled", true, new ConfigDescription("Enable the Trader Fix module. This will force cards with the rare card background to be rare cards and not appear in choice nodes. It will also prevent rare cards from being sold for wolf pelts or lower. Keep in mind that some cards like the Treant don't need to be \"fixed\" and should go to the ignorelist.")).Value;
			config.RareCardIgnorelist = Regex.Split(Config.Bind("TraderFixModule", "RareCardIgnorelist", "Gareth48, Treant, Snag", new ConfigDescription("Cards to ignore when fixing. Entries seperated by commas; any whitespace after a comma is removed. Use the internal name of a card - not its display name.")).Value, @",\s*");
			config.BossModuleEnabled = Config.Bind("BossModule", "BossEnabled", true, new ConfigDescription("Enable the Boss Module. This will tweak boss fights to be harder.")).Value;
			config.Gunbots = Config.Bind("BossModule", "QueenBees", true, new ConfigDescription("Leshy will summon Queen Bees in unopposed spaces during phase 2 of his fight. A queen bee is defined as: 2 power, 1 health, Sentry, Airborne.")).Value;
			config.GunbotCost = Config.Bind("BossModule", "QueenBeeCost", 2, new ConfigDescription("How much blood a queen bee costs.")).Value;
			config.GunbotAttack = Config.Bind("BossModule", "QueenBeeAttack", 3, new ConfigDescription("How much power a queen bee has.")).Value;
			config.GunbotHealth = Config.Bind("BossModule", "QueenBeeHP", 3, new ConfigDescription("How much health a queen bee has.")).Value;
			//config.GunbotsTradeable = Config.Bind("BossModule", "QueenBeesTradeable", true, new ConfigDescription("Whether or not Queen Bees can appear in Golden Pelt trades. Requires QueenBees to be true to work.")).Value;
			config.TradeRareCards = Config.Bind("BossModule", "TradeRareCards", true, new ConfigDescription("If this is true, the Tapper/Trader will offer Rare cards during Phase 2 of his fight.")).Value;
			config.RareRequiresGP = Config.Bind("BossModule", "RareRequiresGP", true, new ConfigDescription("When offering rare cards during Phase 2 of the Trapper/Trader fight, should said rare cards cost Golden Pelts?")).Value;
			//config.ProspectorDontClearQueue = Config.Bind("BossModule", "ProspectorDontClearQueue", true, new ConfigDescription("If this is true, the Prospector will not clear his queue after entering phase 2 of his fight.")).Value;
		}

	}
}
