using DiskCardGame;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LongHaulMod {
	class PlayerNerfModule : ModuleBase {

		class ResetOuroPatch {
			static void Prefix() {
				SaveManager.SaveFile.ouroborosDeaths = 0;
			}
		}

		public PlayerNerfModule(MainPlugin plugin) : base(plugin) { }

		public override void Awake() {
			if(MainPlugin.config.NerfOuroborus) PatchNerfOuro();
		}

		private void PatchNerfOuro() {
			var harmony = new Harmony($"{plugin.Info.Metadata.GUID}.PlayerNerfModule.{nameof(ResetOuroPatch)}");

			var original = typeof(Opponent).GetMethod("CleanUp", BindingFlags.Public | BindingFlags.Instance);
			var prefix = typeof(ResetOuroPatch).GetMethod("Prefix", BindingFlags.NonPublic | BindingFlags.Static);

			harmony.Patch(original, prefix: new HarmonyMethod(prefix));
		}
	}
}
