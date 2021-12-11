using APIPlugin;
using DiskCardGame;
using HarmonyLib;
using Pixelplacement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace LongHaulMod {
	public class BossModule : ModuleBase {

		class AddQueenBeesToLeshyFightPatch {
			static void Prefix() {
				foreach(CardSlot cardSlot in Singleton<BoardManager>.Instance.OpponentSlotsCopy) {
					if(cardSlot.Card == null && cardSlot.opposingSlot.Card == null) {
						instance.plugin.StartCoroutine(Singleton<BoardManager>.Instance.CreateCardInSlot(CardLoader.GetCardByName("QueenBee"), cardSlot, 0.1f, false));
					}
				}
			}
		}
		class BuffMoonPatch {
			static void Prefix(CardInfo info) {
				if (info.name == "!GIANTCARD_MOON") {
					MainPlugin.logger.LogInfo("Buffing moon!");

					int p = MainPlugin.config.MoonPowerBuff;
					int h = MainPlugin.config.MoonHealthBuff;
					info.Mods.Add(new CardModificationInfo(p,h));
				}
			}
		}
		class AddRareCardsToTrapperFightPatch {
			private static CardInfo GetRandomRareCardWithAbility(int randomSeed) {
				CardInfo outp = CardLoader.GetRandomRareCard(CardTemple.Nature);
				// we dont want a card in the blacklist, so get new ones until we get a legal one
				{
					int i;
					for (i = 0; i < 100 && MainPlugin.config.RareBlacklist.Contains(outp.name); i++) {
						outp = CardLoader.GetRandomRareCard(CardTemple.Nature);
					}
					if (i == 99) {
						//FIXME: Unlucky players or those with large blacklists may come across this warning despite there being legal cards
						MainPlugin.logger.LogWarning("Could not find card not in blacklist; is the blacklist too large?");
					}
				}

				CardModificationInfo mod = CardLoader.GetRandomAbilityModForCard(randomSeed, outp, true);
				mod.fromCardMerge = true;
				outp.Mods.Add(mod);

				AbilityInfo ability = AbilitiesUtil.GetInfo(mod.abilities[0]);
				MainPlugin.logger.LogInfo($"Spawning a \"{outp.DisplayedNameEnglish}\" ({outp.name}) with an added {ability.name} sigil.");
				return outp;
			}

			static void Postfix(ref List<CardInfo> __result, int numCards, int randomSeed) {
				System.Random random = new System.Random(randomSeed);

				__result[random.Next(0, numCards)] = GetRandomRareCardWithAbility(randomSeed*200);
			}
		}
		class PreventBuyingRaresWithInferiorPeltsPatch {
			static bool Prefix(TradeCardsForPelts __instance, HighlightedInteractable slot, PlayableCard card) {
				if(card.Info.metaCategories.Contains(CardMetaCategory.Rare)) {
					MainPlugin.logger.LogInfo("Player has tried taking rare card");
					if (Singleton<PlayerHand>.Instance.CardsInHand.Exists((x) => x.Info.name == "PeltGolden")) {
						MainPlugin.logger.LogInfo("Player has golden pelt");

						PlayableCard pelt = Singleton<PlayerHand>.Instance.CardsInHand.Find((PlayableCard x) => x.Info.name == "PeltGolden");
						Singleton<PlayerHand>.Instance.RemoveCardFromHand(pelt);
						pelt.SetEnabled(false);
						pelt.Anim.SetTrigger("fly_off");
						Tween.Position(pelt.transform, pelt.transform.position + new Vector3(0f, 3f, 5f), 0.4f, 0f, Tween.EaseInOut, Tween.LoopType.None, null, delegate ()
						{
							UnityEngine.Object.Destroy(pelt.gameObject);
						}, true);
						card.UnassignFromSlot();
						Tween.Position(card.transform, card.transform.position + new Vector3(0f, 0.25f, -5f), 0.3f, 0f, Tween.EaseInOut, Tween.LoopType.None, null, delegate ()
						{
							UnityEngine.Object.Destroy(card.gameObject);
						}, true);
						__instance.StartCoroutine(Singleton<PlayerHand>.Instance.AddCardToHand(CardSpawner.SpawnPlayableCard(card.Info), new Vector3(0f, 0.5f, -3f), 0f));
						slot.ClearDelegates();
						slot.HighlightCursorType = CursorType.Default;
					} else {
						MainPlugin.logger.LogInfo("Player does not have golden pelt");
						Singleton<TextDisplayer>.Instance.ShowMessage("I only accept Golden Pelts for such a powerful card as that one.");
					}
					return false;
				}
				return true;
			}
		}
		class ProspectorDontClearQueuePatch {
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				var code = new List<CodeInstruction>(instructions);

				for(int i=0; i< code.Count; i++) {
					MainPlugin.logger.LogInfo($"{code[i].opcode} {code[i].operand}");
				}

				return code.AsEnumerable();
			}
		}

		public static BossModule instance;

		public BossModule(MainPlugin plugin) : base(plugin) {
			instance = this;
		}

		public override void Awake() {
			if (MainPlugin.config.QueenBee ) {
				AddQueenBee();
				PatchBeeInLeshyFight();
			}
			if (MainPlugin.config.TradeRareCards) {
				PatchRareCardsInTraderFight();
				if(MainPlugin.config.RareRequiresGP) {
					PatchGPCost();
				}
			}
			if(MainPlugin.config.BuffBaitBucket) {
				_ = new CustomCard("BaitBucket") { abilities = new List<Ability>() { Ability.Reach, Ability.Sharp } };
			}
			PatchBuffMoon();
			//TODO: finish this
			//if(MainPlugin.config.ProspectorDontClearQueue) {
				//PatchProspectorDontClear();
			//}
		}

		private void PatchBuffMoon() {
			var harmony = new Harmony(plugin.Info.Metadata.GUID + ".BossModule." + nameof(BuffMoonPatch));

			var cc_original = typeof(BoardManager).GetMethod("CreateCardInSlot", BindingFlags.Public | BindingFlags.Instance);
			var cc_prefix = typeof(BuffMoonPatch).GetMethod("Prefix", BindingFlags.NonPublic | BindingFlags.Static);

			harmony.Patch(cc_original, prefix: new HarmonyMethod(cc_prefix));
		}

		private void PatchProspectorDontClear() {
			var harmony = new Harmony(plugin.Info.Metadata.GUID + ".BossModule." + nameof(ProspectorDontClearQueuePatch));

			var dc_original = typeof(ProspectorBossOpponent).GetNestedType("<StartNewPhaseSequence>d__4").GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance);
			var dc_transpiler = typeof(ProspectorDontClearQueuePatch).GetMethod("Transpiler", BindingFlags.NonPublic | BindingFlags.Static);

			harmony.Patch(dc_original, transpiler: new HarmonyMethod(dc_transpiler));
		}

		private void PatchGPCost() {
			var harmony = new Harmony(plugin.Info.Metadata.GUID + ".BossModule." + nameof(PreventBuyingRaresWithInferiorPeltsPatch));

			var gp_original = typeof(TradeCardsForPelts).GetMethod("OnTradableSelected", BindingFlags.NonPublic | BindingFlags.Instance);
			var gp_prefix = typeof(PreventBuyingRaresWithInferiorPeltsPatch).GetMethod("Prefix", BindingFlags.NonPublic | BindingFlags.Static);

			harmony.Patch(gp_original, prefix: new HarmonyMethod(gp_prefix));
		}

		private void PatchRareCardsInTraderFight() {
			var harmony = new Harmony(plugin.Info.Metadata.GUID + ".BossModule." + nameof(AddRareCardsToTrapperFightPatch));

			var gentrade_original = typeof(TradeCardsForPelts).GetMethod("GenerateTradeCardsWithCostTier", BindingFlags.NonPublic | BindingFlags.Instance);
			var gentrade_postfix = typeof(AddRareCardsToTrapperFightPatch).GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Static);

			harmony.Patch(gentrade_original, postfix: new HarmonyMethod(gentrade_postfix));
		}

		private void PatchBeeInLeshyFight() {
			var harmony = new Harmony(plugin.Info.Metadata.GUID + ".BossModule." + nameof(AddQueenBeesToLeshyFightPatch));

			var deathcard_original = typeof(LeshyBossOpponent).GetMethod("StartDeathcardPhase", BindingFlags.NonPublic | BindingFlags.Instance);
			var deathcard_prefix = typeof(AddQueenBeesToLeshyFightPatch).GetMethod("Prefix", BindingFlags.NonPublic | BindingFlags.Static);

			harmony.Patch(deathcard_original, prefix: new HarmonyMethod(deathcard_prefix));
		}

		private void AddQueenBee() {
			List<CardMetaCategory> metadata = new List<CardMetaCategory>() {
				CardMetaCategory.Rare
			};

			//if (MainPlugin.config.GunbotsTradeable) metadata.Add(CardMetaCategory.TraderOffer);

			List<Ability> abilities = new List<Ability>() {
				Ability.Flying,
				Ability.Sentry
			};

			List<CardAppearanceBehaviour.Appearance> appearances = new List<CardAppearanceBehaviour.Appearance>() {
				CardAppearanceBehaviour.Appearance.RareCardBackground
			};

			List<Tribe> tribes = new List<Tribe>() { Tribe.Insect };

			string name = "QueenBee";
			string displayName = "Queen Bee";

			// This is what Leshy will say the first time you see this card from a boss drop or a choice node.
			string desc = "The leader of the hive. She will attack preemptively.";

			Texture2D tex = new Texture2D(114, 94);
			tex.LoadImage(File.ReadAllBytes(Path.Combine(plugin.Info.Location.Replace("LongHaulMod.dll", ""), "assets/portrait_queenbee.png")));

			//Texture2D texEm = new Texture2D(114, 94);
			//tex.LoadImage(File.ReadAllBytes(Path.Combine(plugin.Info.Location.Replace("LongHaulMod.dll", ""), "Assets/portrait_queenbee_emission.png")));

			NewCard.Add(name, displayName, MainPlugin.config.QueenBeeAttack, MainPlugin.config.QueenBeeHealth, metadata, CardComplexity.Intermediate, CardTemple.Nature,
				description: desc,
				bloodCost: MainPlugin.config.QueenBeeCost,
				tribes: tribes,
				abilities: abilities,
				appearanceBehaviour: appearances,
				defaultTex: tex 
			);
		}
	}
}
