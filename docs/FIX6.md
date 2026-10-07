# FIX6 Visual

Visual 2D test build based on the verified FIX5 core.

## Added
- 2D Steve in four directions;
- 4-frame walking animation;
- held item rendering;
- bottom 9-slot hotbar;
- 2D block/item textures for grass, dirt, stone, crafting table, wooden pickaxe,
  wooden sword, apple, torch, oak log, planks, stick and cobblestone;
- textured placed blocks.

## Stability approach
Inventory/crafting/save persistence remain disabled in this visual test. All visual
sprite globals are initialized before Draw, and missing external assets degrade
without reading an uninitialized variable.

The runtime assets are loaded from an `undercraft_assets` folder next to
`UNDERTALE.exe`. This avoids repacking the legacy TranslaTale texture pages while
the visual layer is being stabilized.

Test build SHA-256:
`771ba8d277909b57e97574d3ffccd40fb56d87b8f74f087223268913543463b2`.
