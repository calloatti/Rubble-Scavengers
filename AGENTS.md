Include ..\AGENTS.md

# Rubble Scavengers — Mod-Specific Agent Instructions

## Identity
- **Assembly:** `rubblescavengers`
- **Namespace:** `Calloatti.RubbleScavengers`
- **Framework:** Harmony, Bindito DI
- **Publicizer:** includes `Timberborn.RecoveredGoodSystem`, `Timberborn.ConstructionSites`, `Timberborn.InventorySystem`
- **ModId:** `Calloatti.RubbleScavengers`
- **Min Game Version:** 1.0.13.1 — uses `timberborn-decompiled-1.0.*`

## What This Mod Does
Beavers automatically scavenge rubble and recover goods from destroyed buildings. Patches recovered good system and construction site logic to enable passive resource recovery.

## Source Architecture (`Version-1.0/Source/`)

| File | Role |
|---|---|
| `ModStarter.cs` | Entry point — `IModStarter` |
| `ModPatches.cs` | Harmony patches for scavenging behavior |
