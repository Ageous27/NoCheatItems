# NoCheatItems

Server-side BepInEx plugin for Valheim 1.0. It deletes items that the game itself has marked as cheated, plus the old debug weapons, from the **server's** loaded world and from **online** player inventories.

Clients do not need this mod. Install it on the dedicated server (or the listen-server host).

## What it removes

Valheim 1.0 stamps spawned items with a persistent flag (`ItemData.m_cheated`). Those stacks show a grey italic tooltip:

> *This item was summoned through cheating means.*

If a clean character picks one up, the game puts them in a **temporary cheat state** until every flagged item is gone. This plugin deletes those items so they never sit in chests, on the ground, or in a player's bag on your server.

By default it also deletes the vanilla debug prefabs:

- `SwordCheat`
- `SledgeCheat`

Optional (off by default): stacks above max stack size, quality above max quality, and extra prefab names you list in the config.

## What it cannot do

Valheim keeps character files on the **client**. The dedicated server only sees a player's inventory while they are connected. Offline bags are cleaned the next time that player joins.

Unloaded zones are cleaned when they stream in (a chest or dropped pile is scanned as soon as that area is loaded).

This does not un-flag a character or world that already used `devcommands`. It only destroys cheated **items**.

## Install

1. Install [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) on the dedicated server.
2. Copy `NoCheatItems.dll` into `BepInEx/plugins/`.
3. Start the server once to generate `BepInEx/config/ageous27.nocheatitems.cfg`.

## Console

On the server console:

```
nocheatitems_sweep
```

Sweeps every loaded player, container, world drop, and item stand immediately.

## Config

| Setting | Default | Meaning |
| --- | --- | --- |
| `Enabled` | true | Master switch |
| `RemoveFlaggedItems` | true | Delete the 1.0 cheated flag |
| `RemoveKnownCheatPrefabs` | true | Delete SwordCheat / SledgeCheat |
| `ExtraPrefabNames` | empty | Extra prefab names, comma-separated |
| `RemoveOverstacked` | false | Delete illegal stack sizes |
| `RemoveOverquality` | false | Delete illegal upgrade levels |
| `ScanPlayers` | true | Online inventories |
| `ScanWorldDrops` | true | Items on the ground |
| `ScanContainers` | true | Chests, carts, ships, tombstones |
| `ScanItemStands` | true | Item stands and armor stands |
| `ScanIntervalSeconds` | 8 | Safety-net sweep while the world is loaded |
| `NotifyPlayers` | true | Center-screen message when their items are taken |
| `LogDeletions` | true | BepInEx log line per item |
| `DryRun` | false | Log only, do not delete |

Pickup and inventory changes are cleaned immediately; the interval is a backup for stands and anything a patch missed.

## Build

Requires the Valheim 1.0 client (Unity 6 / `6000.0.75f1`) so the project can reference `assembly_valheim.dll`.

```bat
dotnet build -c Release
```

If Valheim is not at `D:\Program Files\Steam\steamapps\common\Valheim`, pass `/p:ValheimPath="C:\path\to\Valheim"`.

Output: `bin\Release\NoCheatItems.dll`.
