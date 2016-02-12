# Psycho Squad
This is my solution to programming challenge from CDPR, written in C# within Unity 5.3.1. This project is for consideration purposes only and cannot be reused in part or whole without express permission of author.

## Requirements
- Create a simple turn-based game prototype featuring basic mechanics in Unity.
- No restrictions on graphical representation.
- Able to start/finish game without crashes.
- Can be ran from editor, but may be tested on Android mobile.

## Tasks
- Tile-based map.
- Entities (friend/foe).
- Definition of a unit w/ basic stats.
- Concept/implementation of turns.
- Unit movement and attacks.
- Special skill mechanics.
- Basic UI for controlling game flow.


## Features
- Basic AI that can navigate scenes using cover, attack opponents, and utilize special abilities appropriately.
- Turn-based gameplay.
- Currently setup for 4v4 teams (v. hard difficulty), however difficulty can scale from easy to very hard, which increments number of enemies on team from 1-4.
- Easy UI interface for both desktop/mobile navigation and interaction with units.
- Custom controls in options for desktop or gamepad usage.
- Player can move and attack or reload once per turn, some specials use move or attack phase.
- Characters have limited sensorium (Line of Sight, sight range, etc) and various stats/personalities (accuracy, health, move range, defensive/neutral/aggressive).
- Weapons have individual stats (range, accuracy, fire rate, etc).
- Player characters all have unique specials and 100 health.
- Enemy NPCs all have dermal armor ability and 300 health.
- Utilizes Unity's NavMesh for pathfinding, but cursor selections are limited to static grid size.
- Blocked visibility of player character/cursor avoided by collision detection setting environment obstructions transparent.
- Cover-based environment and calculations in combat.
- Camera scene navigation in game mode.
- Audio manager script handles/separates game, music, and UI audio settings, and does cross-scene track changes.
- Third party assets used includes cInput for key binding and JMO Assets WarFX for muzzle flashes.

## Notes
- Player units (special):
  1. Assault (Nanite Cloud) - uses mid-range assault rifle, Nanite Cloud immediately heals all friendlies in a small area for a small amount.
  2. Tech (Scan) - uses dual pistols, Scan reveals hidden enemies slightly further than sight range, including LOS-obstructed for several rounds.
  3. CQB (Smart-Link) - uses a silenced SMG, Smart-Link grants perfect accuracy for the current round.
  4. Scout (ThermOptic Camo) - uses long-range sniper rifle, ThermOptic Camo makes unit invisible to enemy and grants sneak attack bonus for the current round.
- Enemy unit
  1. Pyscho (Dermal Armor) - uses a mid-range LMG, Dermal Armor significantly reduces all damage taken.
- Touch controls can be finicky, a slow press and release cycle should help.
- Occasionally some reload actions do not occur until the next turn.
