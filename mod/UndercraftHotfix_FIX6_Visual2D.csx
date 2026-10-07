// Undercraft FIX6 - visual 2D build with external texture assets.
EnsureDataLoaded();
var importGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data) { MainThreadAction = MainThreadAction };

var create = Data.Code.ByName("gml_Object_obj_mainchara_Create_0");
var drawMain = Data.Code.ByName("gml_Object_obj_mainchara_Draw_0");
var drawSteve = Data.Code.ByName("gml_Script_scr_uc_draw_steve");
var drawHotbar = Data.Code.ByName("gml_Script_scr_uc_draw_hotbar");
var drawItem = Data.Code.ByName("gml_Script_scr_uc_draw_item");
var blockDraw = Data.Code.ByName("gml_Object_obj_uc_block_Draw_0");

if (create == null || drawMain == null || drawSteve == null || drawHotbar == null || drawItem == null || blockDraw == null)
{
    ScriptError("Undercraft FIX6: required code entry missing.");
    return;
}

importGroup.QueuePrepend(create, @"
global.uc_anim_tick = 0;
global.uc_spr_steve_down = -1;
global.uc_spr_steve_left = -1;
global.uc_spr_steve_right = -1;
global.uc_spr_steve_up = -1;
global.uc_spr_grass = -1;
global.uc_spr_dirt = -1;
global.uc_spr_stone = -1;
global.uc_spr_crafting = -1;
global.uc_spr_pickaxe = -1;
global.uc_spr_sword = -1;
global.uc_spr_apple = -1;
global.uc_spr_torch = -1;
global.uc_spr_log = -1;
global.uc_spr_planks = -1;
global.uc_spr_stick = -1;
global.uc_spr_cobble = -1;

if (file_exists(""undercraft_assets/steve_down.png"")) global.uc_spr_steve_down = sprite_add(""undercraft_assets/steve_down.png"", 4, 0, 0, 12, 30);
if (file_exists(""undercraft_assets/steve_left.png"")) global.uc_spr_steve_left = sprite_add(""undercraft_assets/steve_left.png"", 4, 0, 0, 12, 30);
if (file_exists(""undercraft_assets/steve_right.png"")) global.uc_spr_steve_right = sprite_add(""undercraft_assets/steve_right.png"", 4, 0, 0, 12, 30);
if (file_exists(""undercraft_assets/steve_up.png"")) global.uc_spr_steve_up = sprite_add(""undercraft_assets/steve_up.png"", 4, 0, 0, 12, 30);

if (file_exists(""undercraft_assets/grass.png"")) global.uc_spr_grass = sprite_add(""undercraft_assets/grass.png"", 1, 0, 0, 8, 8);
if (file_exists(""undercraft_assets/dirt.png"")) global.uc_spr_dirt = sprite_add(""undercraft_assets/dirt.png"", 1, 0, 0, 8, 8);
if (file_exists(""undercraft_assets/stone.png"")) global.uc_spr_stone = sprite_add(""undercraft_assets/stone.png"", 1, 0, 0, 8, 8);
if (file_exists(""undercraft_assets/crafting_table.png"")) global.uc_spr_crafting = sprite_add(""undercraft_assets/crafting_table.png"", 1, 0, 0, 8, 8);
if (file_exists(""undercraft_assets/wood_pickaxe.png"")) global.uc_spr_pickaxe = sprite_add(""undercraft_assets/wood_pickaxe.png"", 1, 0, 0, 8, 8);
if (file_exists(""undercraft_assets/wood_sword.png"")) global.uc_spr_sword = sprite_add(""undercraft_assets/wood_sword.png"", 1, 0, 0, 8, 8);
if (file_exists(""undercraft_assets/apple.png"")) global.uc_spr_apple = sprite_add(""undercraft_assets/apple.png"", 1, 0, 0, 8, 8);
if (file_exists(""undercraft_assets/torch.png"")) global.uc_spr_torch = sprite_add(""undercraft_assets/torch.png"", 1, 0, 0, 8, 8);
if (file_exists(""undercraft_assets/oak_log.png"")) global.uc_spr_log = sprite_add(""undercraft_assets/oak_log.png"", 1, 0, 0, 8, 8);
if (file_exists(""undercraft_assets/planks.png"")) global.uc_spr_planks = sprite_add(""undercraft_assets/planks.png"", 1, 0, 0, 8, 8);
if (file_exists(""undercraft_assets/stick.png"")) global.uc_spr_stick = sprite_add(""undercraft_assets/stick.png"", 1, 0, 0, 8, 8);
if (file_exists(""undercraft_assets/cobblestone.png"")) global.uc_spr_cobble = sprite_add(""undercraft_assets/cobblestone.png"", 1, 0, 0, 8, 8);
");

importGroup.QueueReplace(drawItem, @"
var item = argument0;
var px = argument1;
var py = argument2;
var sc = argument3;
var spr = -1;
if (item == 1) spr = global.uc_spr_grass;
if (item == 2) spr = global.uc_spr_dirt;
if (item == 3) spr = global.uc_spr_stone;
if (item == 4) spr = global.uc_spr_pickaxe;
if (item == 5) spr = global.uc_spr_sword;
if (item == 6) spr = global.uc_spr_apple;
if (item == 7) spr = global.uc_spr_torch;
if (item == 8) spr = global.uc_spr_crafting;
if (item == 10) spr = global.uc_spr_log;
if (item == 11) spr = global.uc_spr_planks;
if (item == 12) spr = global.uc_spr_stick;
if (item == 13) spr = global.uc_spr_cobble;

if (spr >= 0)
{
    draw_sprite_ext(spr, 0, px, py, sc, sc, 0, c_white, 1);
    return 1;
}

draw_set_color(make_color_rgb(110, 110, 110));
draw_rectangle(px - (5 * sc), py - (5 * sc), px + (5 * sc), py + (5 * sc), false);
draw_set_color(c_black);
draw_rectangle(px - (5 * sc), py - (5 * sc), px + (5 * sc), py + (5 * sc), true);
draw_set_color(c_white);
return 1;
");

importGroup.QueueReplace(drawSteve, @"
var sx = argument0;
var sy = argument1;
global.uc_anim_tick += 1;

var walking = 0;
if (instance_exists(obj_time))
{
    if (obj_time.left == 1 || obj_time.right == 1 || obj_time.up == 1 || obj_time.down == 1) walking = 1;
}
var fr = 0;
if (walking == 1) fr = floor(global.uc_anim_tick / 5) mod 4;

var ps = global.uc_spr_steve_down;
if (global.uc_facing == 1) ps = global.uc_spr_steve_left;
if (global.uc_facing == 2) ps = global.uc_spr_steve_right;
if (global.uc_facing == 3) ps = global.uc_spr_steve_up;

var held = global.uc_hotbar[global.uc_slot];
var hx = sx + 9;
var hy = sy - 10;
if (global.uc_facing == 1) hx = sx - 9;
if (global.uc_facing == 2) hx = sx + 9;
if (global.uc_facing == 3)
{
    hx = sx + 7;
    hy = sy - 8;
    if (held != 0) scr_uc_draw_item(held, hx, hy, 1);
}

if (ps >= 0) draw_sprite(ps, fr, sx, sy);

if (held != 0 && global.uc_facing != 3) scr_uc_draw_item(held, hx, hy, 1);
draw_set_color(c_white);
return 1;
");

importGroup.QueueReplace(drawHotbar, @"
var vx = view_xview[0];
var vy = view_yview[0];
var vw = view_wview[0];
var vh = view_hview[0];
var cell = 24;
var bw = (cell * 9) + 6;
var ox = vx + floor((vw - bw) / 2) + 3;
var oy = vy + vh - 31;

draw_set_color(make_color_rgb(24, 24, 24));
draw_rectangle(ox - 4, oy - 4, ox + (cell * 9) + 1, oy + 23, false);
draw_set_color(make_color_rgb(92, 92, 92));
draw_rectangle(ox - 3, oy - 3, ox + (cell * 9), oy + 22, true);

for (var i = 0; i < 9; i += 1)
{
    var xx = ox + (i * cell);
    draw_set_color(make_color_rgb(75, 75, 75));
    draw_rectangle(xx, oy, xx + 20, oy + 20, false);
    draw_set_color(make_color_rgb(35, 35, 35));
    draw_rectangle(xx, oy, xx + 20, oy + 20, true);
    if (i == global.uc_slot)
    {
        draw_set_color(c_white);
        draw_rectangle(xx - 2, oy - 2, xx + 22, oy + 22, true);
        draw_rectangle(xx - 1, oy - 1, xx + 21, oy + 21, true);
    }
    if (global.uc_hotbar[i] != 0) scr_uc_draw_item(global.uc_hotbar[i], xx + 10, oy + 10, 1);
    if (global.uc_count[i] > 1)
    {
        draw_set_color(c_black);
        draw_text(xx + 12, oy + 11, string(global.uc_count[i]));
        draw_set_color(c_white);
        draw_text(xx + 11, oy + 10, string(global.uc_count[i]));
    }
}

var held = global.uc_hotbar[global.uc_slot];
draw_set_color(c_white);
if (held != 0) draw_text(ox, oy - 17, scr_uc_item_name(held));
return 1;
");

importGroup.QueueReplace(blockDraw, @"
var spr = -1;
if (uc_type == 1) spr = global.uc_spr_grass;
if (uc_type == 2) spr = global.uc_spr_dirt;
if (uc_type == 3) spr = global.uc_spr_stone;
if (uc_type == 8) spr = global.uc_spr_crafting;
if (spr >= 0) draw_sprite_ext(spr, 0, x, y, 1.125, 1.125, 0, c_white, 1);
draw_set_color(c_white);
");

importGroup.QueueFindReplace(drawMain, "UNDERCRAFT FIX5", "UNDERCRAFT FIX6 VISUAL");
importGroup.Import();
ScriptMessage("Undercraft FIX6 installed: 2D Steve animation, real item/block textures, bottom hotbar.");
