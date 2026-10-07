# 2D Minecraft rendering specification

Undercraft never renders Minecraft geometry in 3D.

## Conversion categories

| Minecraft source | Undercraft runtime representation |
| --- | --- |
| block texture/model | 2D tile sprite |
| item | 2D icon + optional held overlay |
| player skin | layered 2D directional sprite |
| mob skin/model | 2D directional animation sheet |
| armor | 2D player/entity overlay |
| chest/furnace/crafting table | 2D state sprite |
| projectile | 2D directional sprite |
| particle | 2D frame animation |
| dropped item | 2D icon sprite |
| structure | arrangement of 2D room tiles |
| GUI | 2D UI texture |

## Player and mobs

Runtime directions are front, back, left and right. Mobs do not use live 3D models.
Animations are sprite sequences.

Recommended states:
- idle
- walk
- attack
- hurt
- death
- entity-specific action

## Blocks

Blocks are flattened to tiles. For a classic Undertale room view, the default tile
is based on the most recognizable Minecraft face for that block. Special blocks can
have multiple 2D states, e.g. chest open/closed or furnace off/on.

## Items

Every item has an inventory/hotbar icon. Tools, weapons, food and usable items may
also have a held-item overlay for the player sprite.

## UI

The hotbar is fixed to the bottom of the game view and remains independent from
world coordinates. UI assets can use Minecraft-inspired texture pieces while
remaining compatible with Undertale's 2D draw pipeline.

## Safety/stability rule

No rendering script may assume a global variable or sprite exists before the Create
bootstrap initializes it. Visual systems must degrade to a known placeholder/no-op
instead of throwing a runtime fatal error.
