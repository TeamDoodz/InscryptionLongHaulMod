# LongHaul
Tweaks, features and bugfixes to make the game harder. This mod is highly configurable - if you don't like a feature, disable it.

## Modules

This mod is split into seperate modules that can be enabled/disabled in the config.

### Trader Fix Module
This module will force cards with the rare card background to be rare cards and not appear in choice nodes. It will also prevent rare cards from being sold for wolf pelts or lower. The ignorelist can be used to make this module ignore certain cards. (This is less of a bug and more user error; this issue occurs when a card's metadata is setup improperly).
Additionally, any card marked as rare (that is not on the ignorelist) is forced to have the rare background.

### Boss Module
Makes bosses more difficult. Does the following things:
* Adds a new card called the Queen Bee. The Queen Bee is a rare 2-blood, 3/3 card with Airborne and Sentry sigils.
* Leshy will spawn Queen Bees in unopposed spaces on his side of the board at the beginning of phase 2 of his fight.
* The Moon card now has +1 power and +5 health. This is configurable.
* During phase 2 of his fight, the Trapper/Trader will offer rare cards for sale as well as the normal cards he usually provides. A blacklist can be used to restrict the cards he pulls out.
* These rare cards cost Golden Pelts to purchase.

### Battle Module
Buffs difficulty of all battles, not just bosses. Does the following things:
* All cards played by the opponent have a chance to be:
	* Replaced by a random rare card
	* Given a random new sigil
	* Made combined (stats doubled) 

## I found a bug/error, what do I do?
Please report the issue on LongHaul's Github page and ping me on the Inscryption Modding Discord server (teamdoodz#5281).