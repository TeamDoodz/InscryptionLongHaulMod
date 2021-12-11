using DiskCardGame;
using System.Collections.Generic;
using System.Linq;

namespace LongHaulMod.Utilities {
	public static class Util {

		public static T PickRandom<T>(this IEnumerable<T> self, int seed) {
			int index = SeededRandom.Range(0, self.Count(), seed);
			return self.ElementAt(index);
		}

		public static List<Tribe> AllTribesButSquirrel => new List<Tribe>() { Tribe.Bird, Tribe.Canine, Tribe.Hooved, Tribe.Insect, Tribe.Reptile };

	}
}
