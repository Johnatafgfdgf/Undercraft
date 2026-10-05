# Undercraft architecture

## Goal

Undercraft is a total conversion where Minecraft-style systems become part of
Undertale's actual game state. A change in one system must be visible everywhere
else. Equipping a pickaxe, for example, affects overworld rendering, interactions,
battle logic, menus, dialogue conditions and save/load.

## Central state

The mod will expose one authoritative state model:

```text
UndercraftState
├── player
│   ├── selected_slot
│   ├── held_item
│   ├── armor
│   ├── health
│   └── status
├── hotbar[9]
├── inventory[]
├── recipes[]
├── world_changes[]
├── progression_flags[]
└── dialogue_flags[]
```

Consumers:

```text
                UndercraftState
        ┌──────────┼──────────┐
        ↓          ↓          ↓
    Overworld   Battles    Dialogue
        ↓          ↓          ↓
    Rendering    Menus      Puzzles
        └──────────┼──────────┘
                   ↓
                Save/Load
```

## Subsystems

### Player presentation
Steve is layered rather than baked into one sprite sheet:

1. body / facing / animation frame
2. arm pose
3. held item
4. armor overlays
5. contextual effects

This lets new equipment automatically work in every direction without drawing a
separate full-body sprite for every item combination.

### Inventory and hotbar
The hotbar is the authoritative source for the selected held item. Menus, battle
actions and overworld rendering read the same slot state.

### World blocks
Placed/broken blocks are represented as world deltas rather than destructive room
edits. Entering a room reconstructs its current state from the base room plus saved
deltas.

Each block record must include at minimum:
- room identifier
- grid position
- block type
- placement/broken state
- optional metadata

### Battle bridge
Battles receive a snapshot/reference to UndercraftState. Equipped weapons, food,
armor and special items keep their actual state when the player enters/exits combat.

### Dialogue bridge
Dialogue predicates can inspect Undercraft state, allowing NPCs and cutscenes to
react to equipment, crafted items, placed blocks and prior actions.

### Save integration
Undertale remains responsible for narrative progression. Undercraft adds a namespaced
payload for its own state and validates it during load.

## Toolchain

- Owner-provided Undertale `data.win`
- UndertaleModTool / UndertaleModLib
- Reproducible scripts kept in this repository
- Generated original Undercraft assets

Original Undertale or Minecraft game files must never be committed.

## First implementation slice

The first playable vertical slice should prove the whole bridge:

1. replace overworld player presentation with Steve
2. add a 9-slot hotbar
3. add one block item and one tool
4. show the selected item in Steve's hand
5. allow one controlled block placement/break interaction in an existing room
6. carry selected equipment into one battle
7. make one dialogue branch react to the held item
8. save/load all of the above

Once this slice works, content can scale without creating disconnected systems.
