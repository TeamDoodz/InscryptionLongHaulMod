using DiskCardGame;
using HarmonyLib;
using LongHaulMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LongHaulMod {
	class PlayerNerfModule : ModuleBase {

		class ResetOuroPatch {
			static void Prefix() {
				SaveManager.SaveFile.ouroborosDeaths = 0;
			}
		}
		class ReplaceSquirrelHeadPatch {
			/// <summary>
			/// Field ref for RunState.totemTops.
			/// </summary>
			static AccessTools.FieldRef<RunState, List<Tribe>> totemTopsRef = AccessTools.FieldRefAccess<RunState, List<Tribe>>("totemTops");
			static void Postfix(RunState __instance) {
				// Any squirrel heads in the list of totem tops should be replaced with a random head

				Tribe[] copiedList = new Tribe[totemTopsRef(__instance).Count];
				totemTopsRef(__instance).CopyTo(copiedList);

				for (int i=0; i< totemTopsRef(__instance).Count; i++) {
					if (copiedList[i] == Tribe.Squirrel) copiedList[i] = Util.AllTribesButSquirrel.PickRandom(UnityEngine.Random.Range(-100, 100));
				}
				totemTopsRef(__instance) = copiedList.ToList();

				MainPlugin.logger.LogInfo(totemTopsRef(__instance).ReadableToString("totemTops"));
			}
		}

		public PlayerNerfModule(MainPlugin plugin) : base(plugin) { }

		public override void Awake() {
			if(MainPlugin.config.NerfOuroborus) PatchNerfOuro();
			if (MainPlugin.config.RemoveSquirrelHead) PatchRemoveSquirrelHead();
		}

		private void PatchRemoveSquirrelHead() {
			var harmony = new Harmony($"{plugin.Info.Metadata.GUID}.PlayerNerfModule.{nameof(ReplaceSquirrelHeadPatch)}");

			var original = typeof(RunState).GetMethod("InitializeStarterDeckAndItems", BindingFlags.NonPublic | BindingFlags.Instance);
			var postfix = typeof(ReplaceSquirrelHeadPatch).GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Static);

			harmony.Patch(original, postfix: new HarmonyMethod(postfix));
		}

		private void PatchNerfOuro() {
			var harmony = new Harmony($"{plugin.Info.Metadata.GUID}.PlayerNerfModule.{nameof(ResetOuroPatch)}");

			var original = typeof(Opponent).GetMethod("CleanUp", BindingFlags.Public | BindingFlags.Instance);
			var prefix = typeof(ResetOuroPatch).GetMethod("Prefix", BindingFlags.NonPublic | BindingFlags.Static);

			harmony.Patch(original, prefix: new HarmonyMethod(prefix));
		}
	}
}
