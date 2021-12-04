using APIPlugin;
using DiskCardGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LongHaulMod {
	public class TraderFixModule : ModuleBase {
		public TraderFixModule(MainPlugin plugin) : base(plugin) {
		}

		public override void Start() {
			MainPlugin.logger.LogInfo("Running TraderFixModule...");
			MainPlugin.logger.LogInfo("Keep in mind that TraderOffer only allows cards to be sold with rabbit/wolf pelts; the Trader can sell any (unlocked) rare card regardless of metadata.");

			foreach (var card in NewCard.cards) { // For every modded card...
				if(MainPlugin.config.RareCardIgnorelist.ToList().Contains(card.name)) {
					MainPlugin.logger.LogInfo($"Card \"{card.DisplayedNameEnglish}\" ({card.name}) is on the ignore list. Skipping!");
					continue;
				}

				if(card.metaCategories.Contains(CardMetaCategory.Rare) && !card.appearanceBehaviour.Contains(CardAppearanceBehaviour.Appearance.RareCardBackground))  {
					MainPlugin.logger.LogWarning($"Card \"{card.DisplayedNameEnglish}\" ({card.name}) is marked as rare but deos not have rare background. Fixing!");
				}

				if (card.appearanceBehaviour.Contains(CardAppearanceBehaviour.Appearance.RareCardBackground) && !card.metaCategories.Contains(CardMetaCategory.Rare)) { // ...If it has a rare background but is not a rare...
					MainPlugin.logger.LogWarning($"Card \"{card.DisplayedNameEnglish}\" ({card.name}) has rare background but is not a rare. Fixing!");
					card.metaCategories.Add(CardMetaCategory.Rare); // ...add that metadata.
				}
				
				if(card.metaCategories.Contains(CardMetaCategory.Rare) && card.metaCategories.Contains(CardMetaCategory.ChoiceNode)) {
					MainPlugin.logger.LogWarning($"Card \"{card.DisplayedNameEnglish}\" ({card.name}) has rare metadata but also choice node metadata. Fixing!");
					card.metaCategories.Remove(CardMetaCategory.ChoiceNode);
				}

				if (card.metaCategories.Contains(CardMetaCategory.Rare) && card.metaCategories.Contains(CardMetaCategory.TraderOffer)) {
					MainPlugin.logger.LogWarning($"Card \"{card.DisplayedNameEnglish}\" ({card.name}) has rare metadata but also trader offer metadata. Fixing!");
					card.metaCategories.Remove(CardMetaCategory.ChoiceNode);
				}
			}
		}

	}
}
