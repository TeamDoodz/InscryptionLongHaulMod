using DiskCardGame;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LongHaulMod {
	class BattleModule : ModuleBase {

		class BuffPlayedCardsPatch {
			/// <summary>
			/// If this is true, this modifer will not have an effect. Bit of a hacky way of doing it but whatever. Make sure to turn this back off after calling <see cref="BoardManager.QueueCardForSlot(PlayableCard, CardSlot, float, bool, bool)"/>.
			/// </summary>
			public static bool TempDisable = false;
			static void Prefix(PlayableCard card) {
				if(TempDisable) {
					MainPlugin.logger.LogInfo("BuffPlayedCardsPatch has been temporarily disabled! Make sure to turn it back on after you are done!");
				} else if (!MainPlugin.config.OpponentCardBuffIgnorelist.Contains(card.Info.name)) {
					MainPlugin.logger.LogInfo($"Ready to buff card {card.Info.name}");

					Random rand = new Random();

					CardInfo info;

					if (rand.Next(0, 101) > MainPlugin.config.OpponentRareCardChance) {
						info = (CardInfo)card.Info.Clone();
					} else {
						//FIXME: During totem battles, when replacing a card it keeps its totem sigils
#if KAYCEECOMPAT
						info = CardLoader.GetRandomUnlockedRareCard(rand.Next(0, 1000));
#else
						info = CardLoader.GetRandomRareCard(CardTemple.Nature);
#endif
						int i;
						for (i = 0; i < 100 && MainPlugin.config.OpponentRareCardBlacklist.Contains(info.name); i++) {
#if KAYCEECOMPAT
							info = CardLoader.GetRandomUnlockedRareCard(rand.Next(0, 1000));
#else
							info = CardLoader.GetRandomRareCard(CardTemple.Nature);
#endif
						}
						if (i == 99) {
							//FIXME: Unlucky players or those with large blacklists may come across this warning despite there being legal cards
							MainPlugin.logger.LogWarning("Could not find card not in blacklist; is the blacklist too large?");
						}
					}

					if (rand.Next(0, 101) < MainPlugin.config.OpponentExtraSigilChance) {
						var mod = new CardModificationInfo(GetRandoAbility(info.Abilities, rand.Next(0, 1000))) {
							fromCardMerge = true
						};
						info.Mods.Add(mod);
					}

					if (rand.Next(0, 101) < MainPlugin.config.OpponentCombinedCardChance) {
						var mod = DuplicateMergeSequencer.GetDuplicateMod(info.Attack, info.Health);
						info.Mods.Add(mod);
					}

					//PSA: Do not do card.info = x, instead use card.SetInfo(x)
					card.SetInfo(info);

					//card.RenderCard();
				} else {
					MainPlugin.logger.LogInfo($"Won't buff ignored card {card.Info.name}");
				}
			}

			private static Ability GetRandoAbility(List<Ability> blacklist, int seed) {
#if KAYCEECOMPAT
				Ability outp = AbilitiesUtil.GetRandomLearnedAbility(seed, true, maxPower: MainPlugin.config.OpponentExtraSigilMaxPower);
#else
				Ability outp = AbilitiesUtil.GetRandomAbility(seed,true,true,maxPower:MainPlugin.config.OpponentExtraSigilMaxPower);
#endif
				int i;
				for (i = 0; i < 100 && blacklist.Contains(outp); i++) {
					MainPlugin.logger.LogWarning($"Tried to apply illegal sigil {outp} to card. Trying again!");
#if KAYCEECOMPAT
					outp = AbilitiesUtil.GetRandomLearnedAbility(seed, true, maxPower: MainPlugin.config.OpponentExtraSigilMaxPower);
#else
					outp = AbilitiesUtil.GetRandomAbility(seed,true,true,maxPower:MainPlugin.config.OpponentExtraSigilMaxPower);
#endif
				}
				if (i == 99) {
					//FIXME: Unlucky players or those with large blacklists may come across this warning despite there being legal sigils
					MainPlugin.logger.LogWarning("Could not find sigil not in blacklist; is the blacklist too large?");
				}
				return outp;
			}
		}
		class DisableBuffDuringTraderPhasePatch {
			static void Prefix() {
				BuffPlayedCardsPatch.TempDisable = true;
				MainPlugin.logger.LogInfo("Disabled BuffPlayedCardsPatch");
			}
			static IEnumerator Postfix(IEnumerator enumerator) {
				yield return enumerator;
				BuffPlayedCardsPatch.TempDisable = false;
				MainPlugin.logger.LogInfo("Enabled BuffPlayedCardsPatch");
			}
		}

		public BattleModule(MainPlugin plugin) : base(plugin) {
			instance = this;
		}

		public static BattleModule instance;

		public override void Awake() {
			PatchBuffedOpponentCards();
			PatchTrapperTraderDisabling();
		}

		private void PatchTrapperTraderDisabling() {
			var harmony = new Harmony($"{plugin.Info.Metadata.GUID}.BattleModule.{nameof(DisableBuffDuringTraderPhasePatch)}");

			var original = typeof(TradeCardsForPelts).GetMethod("TradePhase", BindingFlags.Public | BindingFlags.Instance);
			var prefix = typeof(DisableBuffDuringTraderPhasePatch).GetMethod("Prefix", BindingFlags.NonPublic | BindingFlags.Static);
			var postfix = typeof(DisableBuffDuringTraderPhasePatch).GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Static);

			harmony.Patch(original, prefix: new HarmonyMethod(prefix), postfix: new HarmonyMethod(postfix));
		}

		private void PatchBuffedOpponentCards() {
			var harmony = new Harmony($"{plugin.Info.Metadata.GUID}.BattleModule.{nameof(BuffPlayedCardsPatch)}");

			var original = typeof(BoardManager).GetMethod("QueueCardForSlot", BindingFlags.Public | BindingFlags.Instance);
			var prefix = typeof(BuffPlayedCardsPatch).GetMethod("Prefix", BindingFlags.NonPublic | BindingFlags.Static);

			harmony.Patch(original, prefix: new HarmonyMethod(prefix));
		}
	}
}
