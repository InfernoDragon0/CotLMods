# Cult of The Lamb: Mini Mods
Get it at [Thunderstore](https://cult-of-the-lamb.thunderstore.io/package/InfernoDragon0/CotLMiniMods/)
Or [Nexus Mods](https://www.nexusmods.com/cultofthelamb/mods/12)

![text](https://i.imgur.com/o37tDKr.gif)

# Microsite
Read more about this mod at [CotL Mini Mods Site](https://cotlminimod.infernodragon.net/)

# Previous changes (read the microsite for the latest updates)
## Follower Management Device (v1.0.7)
![text](https://i.imgur.com/U3yNhxF.png)
- A new device for you to build in your base
- Lets you spy on your followers
- Assign work commands remotely through the device!

#### End of What's New

# Structures

## Kitchen I
- Allows your followers to cook food in the kitchen!
- If you have a kitchen, food will cost 50% less to cook (minimum 1 per ingredient) [Enabled by default]

## Kitchen II
- An upgrade for your kitchen! You can now queue up to 15 food items (soft cap) in the Kitchen II
- If you queue more than 15 food items, you might not be able to access the kitchen again until it stops overflowing, but the chef can cook regardless.
- Comes with a Divine Bell for a small custom ritual
- Pay 10 Gold to the Divine Bell to consume all food and convert them into 2x Hunger
- If you feed the followers this way, the perks of the food is not provided, but a very fast way to fill hunger.
- You can now pull food out of the Food storages

## Fishing Huts
- You can now build Fishing Huts
- The hut can store up to 75 fishes at once

## Food Storage I & II
- Store additional food in food storages to prevent rotting!

# Roles

## Fisher Role
- You can command a follower to fish via "Follower > Fisher" to let them start fishing
- By default, worker followers will navigate to fishing huts to start fishing (when they fish this way, they only give salmon)
- However, using the command, the custom loot table is enabled: (higher chances) Small fish, Fish, (lower chances) Octopus, Lobster, Squid, Blowfish, Crab

## Waiter Role
- You can command a follower via "Follower > Waiter" to let them bring food to your other followers
- The Waiter will serve the food that is on the floor first
- The Waiter will take food from the storage if there is no food on the floor (The food will be thrown on the floor first)
- If you enable the Waiter Role, followers will not run to the food, but instead wait to be served. (So the followers can work longer)
- If there is no more food to serve, the waiters will wait around the kitchen.
- Waiters will only reposition to the follower if they run too far away from their initial spot (7 units away)
- The food will remain on the floor until the follower finishes the food. If you eat it before it reaches the follower, the follower will not get the food.
- Followers "tip" 2 gold on their location upon receiving food
- Followers hunger satiation is increased 2x when served food
- The waiter teleports food from the kitchen
- The waiter can dump several stacks of food at once on a follower

## Chef Role
- You can assign a chef by commanding "Follower > Work > Chef"
- (IMPORTANT) Upgrade the Cooking Fire to a Kitchen or Kitchen II for the chef to be able to work.
- Select food in the cooking menu, then leave and the Chef will cook it automatically (from 1 minute to 3 minutes, depending on food type)
- A Food bubble will appear on top of the chef if they are currently cooking food.
- The Chef will not leave the kitchen after completing all the orders unless interrupted.
- Food will first be stored in the available Food Storages, then on the floor.


# Extras
- All you can eat Buffet: Players can now eat as many food items as they want. (Deaths due to excessive eating not covered by insurance.) [Enabled by default]
- Large Shrine: Shrine stores 2000 Souls max instead [Enabled by default]
- Instant Collection: Clicking the Shrine once will grab all the souls instead of just 1 [Enabled by default]
- Skips Intro Cutscene [Enabled by default]
- You can now challenge "The One Who Waits" repeatedly and add more of them to your collection. [Disabled by default]
- Config files available for enabling and disabling any part of the mod.

## Known Issues
- If your waiter served another waiter, the other waiter might be interrupted from their waiter role.
- Animations right now are currently very scuffed (e.g. sometimes the chef just holds a fishing rod for some reason), to be fixed in the future
- When you build more than one fishing hut in the same session, everyone might crowd in the same fishing hut. To fix this, just rejoin the game.
- Follower Manager Device: If you assign a task to a follower that cannot be done, they might turn invisible, just re-assign another task.
- There are some parts of the mod that is meant for v1.0.8, such as the extra food mod, it is partially implemented so it will not work right now, but is disabled by default.

## Installation
- Paste the plugins folder in BepInEx folder. This mod consist of CotlMiniMods.dll and an Asset folder.

## Requirements
- BepInEx Pack
- COTL API

## Changelog

### v1.0.7
- Folder structure has been changed, upgrading from 1.0.6 and below manually? delete the old dll first!
- Follower Manager Device has been implemented
- There are some leftover code meant for 1.0.8, check known issues.

### v1.0.6
- Kitchen II has been re-implemented
- The new structures now have icons!
- Kitchen and Kitchen II can now be moved.
- New Divine Bell for the Kitchen II (notes above)
- Food Storage is now interactable

### v1.0.5
- Several Waiter buffs, as shown above
- The waiter will now not be required to return to the kitchen to collect the food, instead, will continue serving directly.
- Followers will now "tip" the waiter 2 gold, dropped on their current location.
- The kitchen now goes on fire when the chef starts to cook food to indicate cooking.
- You can now build fishing huts!
- Fishing drop table list can be seen above
- You can now bring more than 1 The One Who Waits home (ideally you should enable this only after you collected him once before)

### v1.0.4
- Bumped COTL API version to 0.1.4 
- Minor bugfix for Reflection Exception that may occur when loading the mod

### v1.0.3
- Config File has been added to tweak this mod to your liking.
- The chef has been repositioned slightly upwards so that he is not inside the cooking pot.
- The chef now has a bubble to show that they are cooking or not.
- New Role: Waiter has been added.
- Challenge The One Who Waits repeatedly (Enable it in the config file first)
- Challenge the Shrimp Chef Rakshasa (untested) repeatedly (Enable it in the config file first)

### v1.0.2
- Kitchens reduce food cost by 50% (minimum 1 per ingredient)
- Players can now eat as much as they want.

### v1.0.1 / v1.0.0
- Initial Release
