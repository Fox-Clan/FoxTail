# FoxTail for Resonite
A custom headless runner for Resonite with advanced integrations.

> [!WARNING]  
> FoxTail is a little bit early in development.
> It's mostly stable, but I wouldn't trust this for a large event.

## Features

- Full control from the console, Resonite, and Discord!
  You can, for example, start a grid world without ever leaving Resonite,
  or warm up a home world while launching the game from Discord.
- Easily give users permissions on the headless!
  Your friends can do this too, by adding their user IDs in the configuration.
- Quickly start known worlds!
  You can define a list of worlds known by short aliases, instead of dropping in a record URL every time.
- Send friend requests!
  You can tell your headless account to friend a user, instead of having to log into the account.
- Promote yourself in restricted worlds!
  You can run a command to promote yourself to Admin without technically having permission to do so. Though, use this wisely.

## Environment Variables

### Authentication
- `RESO_USER`: Your Resonite account's username. Mostly required.
- `RESO_PASS`: Your Resonite account's password. Mostly required.
- `DISCORD_TOKEN`: A Discord bot token. Enables Discord integration!

### Configuration
- `HEADLESS_RUNTIMES_PATH`: The path to the stock headless client's `runtimes` folder.
  Technically, this isn't required for the engine to run but is definitely recommended.
  You can also copy the runtimes folder next to `FoxTail.exe`.

## Building & Running
TODO... it's a bit of a pain. will probably make docker image that takes a steamcmd key

## Configuration Files
TODO. Most options in the config are self-explanatory.

Take a look at them once FoxTail has spun up for the first time.

## Requirements
- .NET 9 SDK
- Access to the [Resonite Headless Client](https://wiki.resonite.com/Headless_Server_Software) through Patreon/Stripe.
- Patience; this software is still a bit early in development.
