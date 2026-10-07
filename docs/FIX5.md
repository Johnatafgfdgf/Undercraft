# FIX5 verified core

FIX5 fixes the runtime crash where `scr_uc_draw_steve` read `global.uc_slot`
before it had been initialized.

The final compiled `data.win` was decompiled again after build and verified to contain:

- unconditional `global.uc_slot = 0` at the start of `obj_mainchara` Create;
- all 9 hotbar item/count slots initialized before Step/Draw;
- `scr_uc_init`, `scr_uc_save` and `scr_uc_load` reduced to `return 1;`;
- visible build marker `UNDERCRAFT FIX5`.

Final test build SHA-256:
`40ee73e03fddde9d4e3c795347a71d2da0de7d3469a68255b7af4a3347f71f2c`

Temporary stabilization limitations:
- inventory/crafting disabled;
- persistence disabled;
- current-room core/hotbar/block testing remains enabled.
