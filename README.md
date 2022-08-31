# Fantasy Auto Racer

#### *This is a clone of the main repository with all of the licensed art assets stripped out and all of the releases removed*

## Project Layout
- Code is found in [`/src`](https://github.com/jake-small/auto-racer-code-only/tree/main/src) which includes:
  - [`scenes`](https://github.com/jake-small/auto-racer-code-only/tree/main/src/scenes) - Godot Scenes
  - [`scripts`](https://github.com/jake-small/auto-racer-code-only/tree/main/src/scripts) - Godot Scripts
  - [`engine`](https://github.com/jake-small/auto-racer-code-only/tree/main/src/engine) - Engine code with no dependencies on Godot
- Tests are found in [`/AutoRacerTests`](https://github.com/jake-small/auto-racer/tree/main/AutoRacerTests)
  - Only projects with no dependencies on Godot can be unit tested at the time this was created

## Description
![](/fantasy_auto_racer_title_image.PNG)

[Live Demo](https://www.fantasyautoracer.com/ "www.fantasyautoracer.com")

Fantasy Auto Racer is an auto battler, a niche genre with elements of traditional deck builders. Instead of battling, this game re-imagines the genre as a series of races. It's split into two phases. The first phase is spent buying items to improve your character, and the second phase is where you watch your character race. Repeat these phases and continually improve your character until the game is over. How many 1st place finishes can you rack up?

This project was started in order to learn the ins-and-outs of the Godot game engine. It was inspired by [Super Auto Pets](https://teamwoodgames.com/)- a well put together casual auto battler. If you find this game interesting, I highly recommend checking out SAP. After finishing this game, I feel comfortable with Godot and look forward to using it in future projects.

All item data is stored in json and support Lua functions. This lets me easily modify, add, delete, and version items. It also leaves the door open to custom items created by users, though that's not supported with the web release right now.

Project management was done in [Trello](https://trello.com). The best advice I could give to other game developers is to take project management seriously. Spending time curating a backlog and tracking tasks was the biggest reason why I was able to get this game to a finished state.

- Made with the game engine [Godot 3.4.4](https://godotengine.org/)
- Multiplayer uses Google's [Firebase](https://firebase.google.com/) via the [Godot Firebase SDK](https://github.com/GodotNuts/GodotFirebase)
- Scripting uses [Lua](https://www.lua.org/) and is interpreted with [MoonSharp](https://www.moonsharp.org/)
- Written primarily in C# with some GDScript
- Pixel art by [FinalBossBlues](http://www.timefantasy.net/)
- UI elements by [Kenney](https://www.kenney.nl/)
