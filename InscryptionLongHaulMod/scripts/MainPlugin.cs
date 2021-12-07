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
			internal bool TraderFixEnabled;
			public string[] RareCardIgnorelist;
			public bool ForceRareBG;
			public bool BossModuleEnabled;
			public bool QueenBee;
			public bool GunbotsTradeable;
			internal bool TradeRareCards;
			internal bool RareRequiresGP;
			internal int QueenBeeCost;
			internal int QueenBeeAttack;
			internal int QueenBeeHealth;
			public int MoonPowerBuff;
			public int MoonHealthBuff;
			internal bool RareInscribed;
			internal string[] RareBlacklist;
			internal bool BattleModuleEnabled;
			internal float OpponentRareCardChance;
			internal string[] OpponentRareCardBlacklist;
			internal float OpponentExtraSigilChance;
			internal int OpponentExtraSigilMaxPower;
			internal float OpponentCombinedCardChance;
			internal string[] OpponentCardBuffIgnorelist;
			internal bool PlayerNerfModuleEnabled;
			internal bool NerfOuroborus;
		}

		private const string GUID = "io.github.TeamDoodz.LongHaulMod";
		private const string Name = "LongHaulMod";
		private const string Version = "1.1.0";

		public static ConfigOptions config;

		public static BepInEx.Logging.ManualLogSource logger;

		private TraderFixModule tf;
		private BossModule bom;
		private BattleModule bam;
		private PlayerNerfModule pm;

		private void Awake() {
			Logger.LogInfo($"{Name} has been awoken");
			logger = Logger;

			GetConfig();

			if (config.TraderFixEnabled) tf = new TraderFixModule(this);
			if(config.BossModuleEnabled) bom = new BossModule(this);
			if (config.BattleModuleEnabled) bam = new BattleModule(this);
			if (config.PlayerNerfModuleEnabled) pm = new PlayerNerfModule(this);

			if (config.BossModuleEnabled) bom.Awake();
			if (config.BattleModuleEnabled) bam.Awake();
			if (config.PlayerNerfModuleEnabled) pm.Awake();
		}

		private void Start() {
			Logger.LogInfo("Start called");

			if(config.TraderFixEnabled) tf.Start();
		}

		private void GetConfig() {
			Logger.LogInfo($"Reading config..."); 

			config.TraderFixEnabled = Config.Bind("TraderFixModule", "TraderFixEnabled", true, new ConfigDescription("Enable the Trader Fix module. This will force cards with the rare card background to be rare cards and not appear in choice nodes. It will also prevent rare cards from being sold for wolf pelts or lower. Keep in mind that some cards like the Treant don't need to be \"fixed\" and should go to the ignorelist.")).Value;
			
			config.RareCardIgnorelist = Regex.Split(Config.Bind("TraderFixModule", "RareCardIgnorelist", "Gareth48, Garethmod_Treant, Garethmod_Snag, beast_2, beast_3, caninegod, hoovedgod", new ConfigDescription("Cards to ignore when fixing. Entries seperated by commas; any whitespace after a comma is removed. Use the internal name of a card - not its display name.")).Value, @",\s*");
			config.ForceRareBG = Config.Bind("TraderFixModule", "ForceRareBG", true, new ConfigDescription("Forces rare cards to have the rare background.")).Value;

			config.BossModuleEnabled = Config.Bind("BossModule", "BossEnabled", true, new ConfigDescription("Enable the Boss Module. This will tweak boss fights to be harder.")).Value;

			config.QueenBee = Config.Bind("BossModule", "QueenBees", true, new ConfigDescription("Leshy will summon Queen Bees in unopposed spaces during phase 2 of his fight. A queen bee is defined as: 2 power, 1 health, Sentry, Airborne.")).Value;
			config.QueenBeeCost = Config.Bind("BossModule", "QueenBeeCost", 2, new ConfigDescription("How much blood a queen bee costs.")).Value;
			config.QueenBeeAttack = Config.Bind("BossModule", "QueenBeeAttack", 3, new ConfigDescription("How much power a queen bee has.")).Value;
			config.QueenBeeHealth = Config.Bind("BossModule", "QueenBeeHP", 3, new ConfigDescription("How much health a queen bee has.")).Value;

			config.MoonPowerBuff = Config.Bind("BossModule", "MoonPowerBuff", 1, new ConfigDescription("Adds this amount to the Moon's power.")).Value;
			config.MoonHealthBuff = Config.Bind("BossModule", "MoonHealthBuff", 5, new ConfigDescription("Adds this amount to the Moon's health.")).Value;

			config.TradeRareCards = Config.Bind("BossModule", "TradeRareCards", true, new ConfigDescription("If this is true, the Tapper/Trader will offer Rare cards during Phase 2 of his fight.")).Value;
			config.RareRequiresGP = Config.Bind("BossModule", "RareRequiresGP", true, new ConfigDescription("When offering rare cards during Phase 2 of the Trapper/Trader fight, should said rare cards cost Golden Pelts?")).Value;
			config.RareInscribed = Config.Bind("BossModule", "RareInscribed", true, new ConfigDescription("If this is true, rare cards offered by the Trader boss will gain an extra sigil, similarly to the regular cards.")).Value;
			config.RareBlacklist = Regex.Split(Config.Bind("BossModule", "RareBlacklist", "Amoeba, MontyPython", new ConfigDescription("Rare cards that shouldn't be played by the Trapper/Trader during his fight. Entries seperated by commas; any whitespace after a comma is removed. Use the internal name of a card - not its display name.")).Value, @",\s*");
			//config.ProspectorDontClearQueue = Config.Bind("BossModule", "ProspectorDontClearQueue", false, new ConfigDescription("(UNFINISHED FEATURE - WILL CREATE ERRORS.) If this is true, the Prospector will not clear his queue after entering phase 2 of his fight.")).Value;

			config.BattleModuleEnabled = Config.Bind("BattleModule", "BattleEnabled", true, "Enables the Battle Module. This one makes all battles, not just bosses, more difficult.").Value;
			
			config.OpponentRareCardChance = Config.Bind("BattleModule", "OpponentRareCardChance", 6.25f, "Percent chance for any card the opponent plays to be replaced with a random rare one.").Value;
			config.OpponentRareCardBlacklist = Regex.Split(Config.Bind("BattleModule", "OpponentRareCardBlacklist", "Amoeba, MontyPython", "Rare cards that the opponent will not play. Entries seperated by commas; any whitespace after a comma is removed. Use the internal name of a card - not its display name.").Value, @",\s*");
			config.OpponentExtraSigilChance = Config.Bind("BattleModule", "OpponentExtraSigilChance", 12.5f, "Percent chance for any card the opponent plays to gain a random sigil.").Value;
			config.OpponentExtraSigilMaxPower = Config.Bind("BattleModule", "OpponentExtraSigilMaxPower", 4, "Maximum level of power sigil to apply to a card, on a scale of 0 to 5.").Value;
			config.OpponentCombinedCardChance = Config.Bind("BattleModule", "OpponentCombinedCardChance", 6.25f, "Percent chance for any card the opponent plays to be a combined card with double stats.").Value;
			config.OpponentCardBuffIgnorelist = Regex.Split(Config.Bind("BattleModule", "OpponentCardBuffIgnorelist", "Mule, BaitBucket, !DEATHCARD_BASE", "List of cards that will not gain extra buffs. Note: Removing !DEATHCARD_BASE from this list can cause issues! Entries seperated by commas; any whitespace after a comma is removed. Use the internal name of a card - not its display name.").Value, @",\s*");

			config.PlayerNerfModuleEnabled = Config.Bind("PlayerNerfModule", "PlayerNerfModuleEnabled", true, "Enable the Player Nerf Module. This module nerfs player-specific cards and actions.").Value;
			config.NerfOuroborus = Config.Bind("PlayerNerfModule", "PlayerNerfModuleEnabled", true, "If this is true, the stats of the Ouroborus will reset after every battle.").Value;
		}

	}
}
