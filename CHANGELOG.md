# Changelog

## 1.0.2
- Delete cheated creature loot on the dedicated server. Vanilla sets `m_cheated` after `ItemDrop.Awake`, so the old check ran too early and never saw the flag. Also reloads ground piles from the ZDO on the periodic sweep.
- Inventory `Changed` patch matches the 1.0.14 `(bool, bool)` signature.

## 1.0.1
- Plugin GUID is now `ageous27.nocheatitems` (config file: `BepInEx/config/ageous27.nocheatitems.cfg`).

## 1.0.0
- Initial release for Valheim 1.0.
- Deletes items marked with the official cheated flag (grey italic tooltip: *This item was summoned through cheating means.*).
- Also removes vanilla debug weapons `SwordCheat` and `SledgeCheat`.
- Cleans online player inventories, loaded containers, world drops, and item/armor stands.
- Optional overstack / over-quality deletion and extra prefab names.
- Console command `nocheatitems_sweep`.
