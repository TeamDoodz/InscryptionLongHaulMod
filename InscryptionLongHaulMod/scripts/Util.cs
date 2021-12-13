using DiskCardGame;
using System.Collections.Generic;
using System.Linq;

namespace LongHaulMod.Utilities {
	public static class Util {

		public static T PickRandom<T>(this IEnumerable<T> self, int seed) {
			int index = SeededRandom.Range(0, self.Count(), seed);
			return self.ElementAt(index);
		}

		public static string ReadableToString<T>(this IList<T> self, string listName) {
			string outp = $"\n---BEGIN CONTENTS OF {listName.ToUpper()}---";
			foreach(var i in self) {
				outp += $"\n{i}";
			}
			outp += $"\n---END CONTENTS OF {listName.ToUpper()}---";
			return outp;
		}

		public static List<Tribe> AllTribesButSquirrel => new List<Tribe>() { Tribe.Bird, Tribe.Canine, Tribe.Hooved, Tribe.Insect, Tribe.Reptile };

	}
}
