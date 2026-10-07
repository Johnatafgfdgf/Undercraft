# FIX4 verified core

FIX4 was built specifically to eliminate the repeated bytecode-15 crash inside
`scr_uc_inventory_init`.

Verification performed on the final `data.win`:

- `gml_Object_obj_mainchara_Step_0` contains no inventory/crafting/bag references.
- `gml_Script_scr_uc_inventory_init` decompiles to `return 1;`.
- `scr_uc_load` and `scr_uc_save` do not reference bag, armor or world arrays.
- The main character Draw event renders the visible text `UNDERCRAFT FIX4`.
- Final SHA-256:
  `a6ca741f6bf74f725989ce5e0daf113b6486c17a34404201892d57916becf528`.

Temporary FIX4 limitations:
- inventory/crafting disabled;
- block persistence between rooms disabled;
- core Steve/hotbar/current-room block testing remains enabled.
