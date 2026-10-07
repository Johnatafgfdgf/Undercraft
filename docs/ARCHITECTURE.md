# Undercraft architecture

## Goal

Undercraft is a total conversion where Minecraft-style systems become part of
Undertale's actual game state. A change in one system must be visible everywhere
else. Equipping a pickaxe, for example, affects overworld rendering, interactions,
battle logic, menus, dialogue conditions and save/load.

## Rendering rule: Minecraft becomes 2D

Undertale remains a 2D game. No Minecraft 3D model is rendered directly.

Anything that is 3D in Minecraft is converted to a 2D representation before it
enters the game:

- player: directional 2D sprite layers;
- mobs/entities: directional sprite sheets with idle/walk/attack/hurt frames;
- blocks: 2D tile sprites/icons, never cubes rendered in 3D;
- held tools/weapons: 2D overlays attached to the hand;
- armor: 2D body overlays;
- block entities such as chests/furnaces/crafting tables: 2D state sprites;
- structures: compositions of 2D room tiles;
- particles/effects: 2D animated frames;
- dropped items: 2D item sprites;
- GUI/hotbar/inventory: native 2D UI.

Minecraft textures are source material for the conversion pipeline, not a reason
to introduce a 3D renderer.

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
Steve is 2D and layered rather than baked into one sprite sheet:

1. 2D body / facing / animation frame
2. 2D arm pose
3. 2D held item
4. 2D armor overlays
5. 2D contextual effects

This lets new equipment work in every direction without drawing a separate
full-body sprite for every item combination.

### Entity presentation
Minecraft mobs are converted to 2D sprite families. Minimum directional set:

- front
- back
- left
- right

Animation states may include idle, walk, attack, hurt, death and special actions.
The exact number of frames can vary per entity, but runtime rendering remains 2D.

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

Blocks render as 2D tile sprites. There is no cube projection or 3D camera.

### Battle bridge
Battles receive a snapshot/reference to UndercraftState. Equipped weapons, food,
armor and special items keep their actual state when the player enters/exits combat.

### Dialogue bridge
Dialogue predicates can inspect Undercraft state, allowing NPCs and cutscenes to
react to equipment, crafted items, placed blocks and prior actions.

### Save integration
Undertale remains responsible for narrative progression. Undercraft adds a
namespaced payload for its own state and validates it during load.

## Asset pipeline

Minecraft texture sources are kept outside the repository and converted locally.

Preferred source:
- Mojang `bedrock-samples/resource_pack/textures`

Reference/fallback:
- `KygekDev/default-textures`

The local converter prepares:
- block tiles
- item icons
- hotbar/UI pieces
- entity source textures
- particles
- environment textures

3D entity/model data is never imported as runtime geometry. If model information is
needed, it is used only as a reference to create 2D directional sprites.

## Toolchain

- Owner-provided Undertale `data.win`
- UndertaleModTool / UndertaleModLib
- Reproducible scripts kept in this repository
- Local Minecraft texture cache
- Generated Undercraft 2D assets

Original Undertale or Minecraft game files/assets must never be committed.

## First implementation slice

The first stable visual slice should prove:

1. replace overworld player presentation with 2D Steve
2. add a 9-slot hotbar fixed at the bottom
3. render real 2D block/item textures
4. show the selected 2D item in Steve's hand
5. animate Steve in four directions
6. allow controlled block placement/break interaction
7. carry selected equipment into one battle
8. preserve stability before re-enabling complex inventory/crafting/save logic

Once this slice works without runtime errors, content can scale without creating
disconnected systems.
