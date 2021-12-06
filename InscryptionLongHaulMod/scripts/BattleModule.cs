using DiskCardGame;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LongHaulMod {
	class BattleModule : ModuleBase {

		class BuffPlayedCardsPatch {
			static void Prefix(PlayableCard card) {
				if (!MainPlugin.config.OpponentCardBuffIgnorelist.Contains(card.Info.name)) {
					Random rand = new Random();

					CardInfo info;

					if (rand.Next(0, 101) > MainPlugin.config.OpponentRareCardChance) {
						info = (CardInfo)card.Info.Clone();
					} else {
						info = CardLoader.GetRandomRareCard(CardTemple.Nature);
						int i;
						for (i = 0; i < 100 && MainPlugin.config.OpponentRareCardBlacklist.Contains(info.name); i++) {
							info = CardLoader.GetRandomRareCard(CardTemple.Nature);
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
				Ability outp = AbilitiesUtil.GetRandomAbility(seed,true,true,maxPower:5);
				int i;
				for (i = 0; i < 100 && blacklist.Contains(outp); i++) {
					MainPlugin.logger.LogWarning($"Tried to apply illegal sigil {outp} to card. Trying again!");
					outp = AbilitiesUtil.GetRandomAbility(seed, true, true, maxPower: 5);
				}
				if (i == 99) {
					//FIXME: Unlucky players or those with large blacklists may come across this warning despite there being legal sigils
					MainPlugin.logger.LogWarning("Could not find sigil not in blacklist; is the blacklist too large?");
				}
				return outp;
			}
		}

		public BattleModule(MainPlugin plugin) : base(plugin) {
			instance = this;
		}

		public static BattleModule instance;

		public override void Awake() {
			PatchBuffedOpponentCards();
		}

		private void PatchBuffedOpponentCards() {
			var harmony = new Harmony($"{plugin.Info.Metadata.GUID}.BattleModule.{nameof(BuffPlayedCardsPatch)}");

			var original = typeof(BoardManager).GetMethod("QueueCardForSlot", BindingFlags.Public | BindingFlags.Instance);
			var prefix = typeof(BuffPlayedCardsPatch).GetMethod("Prefix", BindingFlags.NonPublic | BindingFlags.Static);

			harmony.Patch(original, prefix: new HarmonyMethod(prefix));
		}
	}
}
