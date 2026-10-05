// Undercraft bootstrap installer
// Target: UNDERTALE data.win build registered in mod/manifest.json
// Requires UndertaleModTool / UndertaleModCli 0.9.2.x

EnsureDataLoaded();

if (Data.GeneralInfo == null || Data.GeneralInfo.Name == null ||
    !Data.GeneralInfo.Name.Content.StartsWith("UNDERTALE"))
{
    ScriptError("Undercraft: this script must be applied to UNDERTALE data.win.");
    return;
}

if (Data.Scripts.ByName("scr_uc_init") != null)
{
    ScriptError("Undercraft: bootstrap is already installed in this data.win. Start from a clean copy to re-apply.");
    return;
}

var mainchara = Data.GameObjects.ByName("obj_mainchara");
var battlecontroller = Data.GameObjects.ByName("obj_battlecontroller");
var solidparent = Data.GameObjects.ByName("obj_solidparent");

if (mainchara == null || battlecontroller == null || solidparent == null)
{
    ScriptError("Undercraft: expected UNDERTALE objects were not found. This game build is not supported yet.");
    return;
}

void AddScript(string name, string gml)
{
    var code = new UndertaleCode()
    {
        Name = Data.Strings.MakeString("gml_Script_" + name)
    };
    code.AppendGML(gml, Data);
    Data.Code.Add(code);
    Data.CodeLocals.Add(new UndertaleCodeLocals() { Name = code.Name });
    Data.Scripts.Add(new UndertaleScript()
    {
        Name = Data.Strings.MakeString(name),
        Code = code
    });
    Data.Functions.EnsureDefined(name, Data.Strings);
}

// Register our script names before compiling bodies that reference each other.
string[] ucFunctions = new string[]
{
    "scr_uc_init",
    "scr_uc_load",
    "scr_uc_save",
    "scr_uc_item_name",
    "scr_uc_item_color",
    "scr_uc_item_is_block",
    "scr_uc_draw_item",
    "scr_uc_draw_steve",
    "scr_uc_draw_hotbar",
    "scr_uc_target_x",
    "scr_uc_target_y",
    "scr_uc_place_block",
    "scr_uc_break_block",
    "scr_uc_spawn_room_blocks",
    "scr_uc_use_selected",
    "scr_uc_battle_draw"
};
foreach (string fn in ucFunctions)
    Data.Functions.EnsureDefined(fn, Data.Strings);

AddScript("scr_uc_item_name", @"
var item = argument0;
if (item == 1) return ""Grass Block"";
if (item == 2) return ""Dirt Block"";
if (item == 3) return ""Stone Block"";
if (item == 4) return ""Wooden Pickaxe"";
if (item == 5) return ""Wooden Sword"";
if (item == 6) return ""Apple"";
if (item == 7) return ""Torch"";
if (item == 8) return ""Crafting Table"";
if (item == 9) return ""Empty"";
return ""Empty"";
");

AddScript("scr_uc_item_color", @"
var item = argument0;
if (item == 1) return make_color_rgb(76, 153, 65);
if (item == 2) return make_color_rgb(121, 85, 58);
if (item == 3) return make_color_rgb(124, 124, 124);
if (item == 4) return make_color_rgb(170, 125, 72);
if (item == 5) return make_color_rgb(196, 150, 82);
if (item == 6) return make_color_rgb(204, 42, 42);
if (item == 7) return make_color_rgb(240, 177, 48);
if (item == 8) return make_color_rgb(157, 102, 54);
return c_black;
");

AddScript("scr_uc_item_is_block", @"
var item = argument0;
return ((item == 1) || (item == 2) || (item == 3) || (item == 8));
");

AddScript("scr_uc_save", @"
ini_open(""undercraft.ini"");
ini_write_real(""player"", ""selected_slot"", global.uc_selected_slot);
ini_write_real(""player"", ""facing"", global.uc_facing);
var i;
for (i = 0; i < 9; i += 1)
{
    ini_write_real(""hotbar"", ""slot_"" + string(i), global.uc_hotbar[i]);
    ini_write_real(""hotbar"", ""count_"" + string(i), global.uc_hotbar_count[i]);
}
ini_write_real(""world"", ""block_count"", global.uc_block_count);
for (i = 0; i < global.uc_block_count; i += 1)
{
    ini_write_real(""block_"" + string(i), ""room"", global.uc_block_room[i]);
    ini_write_real(""block_"" + string(i), ""x"", global.uc_block_x[i]);
    ini_write_real(""block_"" + string(i), ""y"", global.uc_block_y[i]);
    ini_write_real(""block_"" + string(i), ""type"", global.uc_block_type[i]);
}
ini_close();
return 1;
");

AddScript("scr_uc_load", @"
ini_open(""undercraft.ini"");
global.uc_selected_slot = ini_read_real(""player"", ""selected_slot"", 0);
global.uc_facing = ini_read_real(""player"", ""facing"", 0);
var i;
for (i = 0; i < 9; i += 1)
{
    global.uc_hotbar[i] = ini_read_real(""hotbar"", ""slot_"" + string(i), global.uc_hotbar[i]);
    global.uc_hotbar_count[i] = ini_read_real(""hotbar"", ""count_"" + string(i), global.uc_hotbar_count[i]);
}
global.uc_block_count = ini_read_real(""world"", ""block_count"", 0);
if (global.uc_block_count < 0) global.uc_block_count = 0;
if (global.uc_block_count > 128) global.uc_block_count = 128;
for (i = 0; i < global.uc_block_count; i += 1)
{
    global.uc_block_room[i] = ini_read_real(""block_"" + string(i), ""room"", -1);
    global.uc_block_x[i] = ini_read_real(""block_"" + string(i), ""x"", 0);
    global.uc_block_y[i] = ini_read_real(""block_"" + string(i), ""y"", 0);
    global.uc_block_type[i] = ini_read_real(""block_"" + string(i), ""type"", 1);
}
ini_close();
return 1;
");

AddScript("scr_uc_init", @"
if (!variable_global_exists(""uc_initialized""))
{
    global.uc_initialized = 1;
    global.uc_selected_slot = 0;
    global.uc_facing = 0;
    global.uc_block_count = 0;
    global.uc_hotbar[0] = 1;
    global.uc_hotbar[1] = 2;
    global.uc_hotbar[2] = 3;
    global.uc_hotbar[3] = 4;
    global.uc_hotbar[4] = 5;
    global.uc_hotbar[5] = 6;
    global.uc_hotbar[6] = 7;
    global.uc_hotbar[7] = 8;
    global.uc_hotbar[8] = 9;
    global.uc_hotbar_count[0] = 64;
    global.uc_hotbar_count[1] = 64;
    global.uc_hotbar_count[2] = 64;
    global.uc_hotbar_count[3] = 1;
    global.uc_hotbar_count[4] = 1;
    global.uc_hotbar_count[5] = 3;
    global.uc_hotbar_count[6] = 16;
    global.uc_hotbar_count[7] = 1;
    global.uc_hotbar_count[8] = 0;
    scr_uc_load();
}
return 1;
");

AddScript("scr_uc_target_x", @"
var px = obj_mainchara.x;
if (global.uc_facing == 1) px -= 20;
if (global.uc_facing == 2) px += 20;
return round(px / 20) * 20;
");

AddScript("scr_uc_target_y", @"
var py = obj_mainchara.y;
if (global.uc_facing == 0) py += 20;
if (global.uc_facing == 3) py -= 20;
return round(py / 20) * 20;
");

// Create the physical block object. It inherits UNDERTALE's normal solid parent so
// existing overworld collision checks see placed blocks as native obstacles.
var ucBlock = new UndertaleGameObject()
{
    Name = Data.Strings.MakeString("obj_uc_block"),
    ParentId = solidparent,
    Sprite = Data.Sprites.ByName("spr_blconsm"),
    Visible = true,
    Solid = true,
    Persistent = false,
    Depth = 0
};
Data.GameObjects.Add(ucBlock);

UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data)
{
    MainThreadAction = MainThreadAction
};

importGroup.QueueReplace(ucBlock.EventHandlerFor(EventType.Create, Data), @"
uc_type = 1;
image_alpha = 0;
depth = 0;
");

importGroup.QueueReplace(ucBlock.EventHandlerFor(EventType.Draw, Data), @"
var c = scr_uc_item_color(uc_type);
draw_set_alpha(1);
draw_set_color(c);
draw_rectangle(x - 9, y - 9, x + 9, y + 9, false);
if (uc_type == 1)
{
    draw_set_color(make_color_rgb(103, 70, 46));
    draw_rectangle(x - 9, y - 3, x + 9, y + 9, false);
    draw_set_color(make_color_rgb(87, 173, 73));
    draw_rectangle(x - 9, y - 9, x + 9, y - 4, false);
}
else if (uc_type == 2)
{
    draw_set_color(make_color_rgb(91, 60, 43));
    draw_rectangle(x - 7, y - 7, x - 2, y - 2, false);
    draw_rectangle(x + 2, y + 1, x + 7, y + 6, false);
}
else if (uc_type == 3)
{
    draw_set_color(make_color_rgb(96, 96, 96));
    draw_rectangle(x - 7, y - 6, x - 1, y - 1, false);
    draw_set_color(make_color_rgb(153, 153, 153));
    draw_rectangle(x + 1, y + 1, x + 7, y + 6, false);
}
else if (uc_type == 8)
{
    draw_set_color(make_color_rgb(91, 54, 29));
    draw_rectangle(x - 7, y - 7, x + 7, y + 7, true);
    draw_line(x, y - 7, x, y + 7);
    draw_line(x - 7, y, x + 7, y);
}
draw_set_color(c_white);
");

AddScript("scr_uc_spawn_room_blocks", @"
var i;
for (i = 0; i < global.uc_block_count; i += 1)
{
    if (global.uc_block_room[i] == room)
    {
        var bx = global.uc_block_x[i];
        var by = global.uc_block_y[i];
        if (instance_position(bx, by, obj_uc_block) == noone)
        {
            var b = instance_create(bx, by, obj_uc_block);
            b.uc_type = global.uc_block_type[i];
        }
    }
}
return 1;
");

AddScript("scr_uc_place_block", @"
var slot = global.uc_selected_slot;
var item = global.uc_hotbar[slot];
if (!scr_uc_item_is_block(item)) return 0;
if (global.uc_hotbar_count[slot] <= 0) return 0;
if (global.uc_block_count >= 128) return 0;
var tx = scr_uc_target_x();
var ty = scr_uc_target_y();
if (point_distance(obj_mainchara.x, obj_mainchara.y, tx, ty) < 12) return 0;
if (instance_position(tx, ty, obj_uc_block) != noone) return 0;
var b = instance_create(tx, ty, obj_uc_block);
b.uc_type = item;
global.uc_block_room[global.uc_block_count] = room;
global.uc_block_x[global.uc_block_count] = tx;
global.uc_block_y[global.uc_block_count] = ty;
global.uc_block_type[global.uc_block_count] = item;
global.uc_block_count += 1;
global.uc_hotbar_count[slot] -= 1;
scr_uc_save();
return 1;
");

AddScript("scr_uc_break_block", @"
var tx = scr_uc_target_x();
var ty = scr_uc_target_y();
var b = instance_position(tx, ty, obj_uc_block);
if (b == noone) return 0;
var broken_type = b.uc_type;
var held_item = global.uc_hotbar[global.uc_selected_slot];
if ((broken_type == 3) && (held_item != 4)) return 0;
var i;
var found = -1;
for (i = 0; i < global.uc_block_count; i += 1)
{
    if ((global.uc_block_room[i] == room) && (global.uc_block_x[i] == tx) && (global.uc_block_y[i] == ty))
    {
        found = i;
        i = global.uc_block_count;
    }
}
if (found >= 0)
{
    for (i = found; i < global.uc_block_count - 1; i += 1)
    {
        global.uc_block_room[i] = global.uc_block_room[i + 1];
        global.uc_block_x[i] = global.uc_block_x[i + 1];
        global.uc_block_y[i] = global.uc_block_y[i + 1];
        global.uc_block_type[i] = global.uc_block_type[i + 1];
    }
    global.uc_block_count -= 1;
}
var return_slot = -1;
for (i = 0; i < 9; i += 1)
{
    if (global.uc_hotbar[i] == broken_type)
    {
        return_slot = i;
        i = 9;
    }
}
if (return_slot >= 0) global.uc_hotbar_count[return_slot] += 1;
with (b) instance_destroy();
scr_uc_save();
return 1;
");

AddScript("scr_uc_draw_item", @"
var item = argument0;
var px = argument1;
var py = argument2;
var scale = argument3;
var c = scr_uc_item_color(item);
draw_set_color(c);
if (scr_uc_item_is_block(item))
{
    draw_rectangle(px - (4 * scale), py - (4 * scale), px + (4 * scale), py + (4 * scale), false);
    draw_set_color(c_black);
    draw_rectangle(px - (4 * scale), py - (4 * scale), px + (4 * scale), py + (4 * scale), true);
}
else if ((item == 4) || (item == 5))
{
    draw_set_color(make_color_rgb(116, 75, 43));
    draw_rectangle(px - scale, py, px + scale, py + (6 * scale), false);
    draw_set_color(c);
    if (item == 4)
    {
        draw_rectangle(px - (5 * scale), py - (4 * scale), px + (5 * scale), py - (2 * scale), false);
        draw_rectangle(px - scale, py - (4 * scale), px + scale, py + scale, false);
    }
    else
    {
        draw_rectangle(px - scale, py - (8 * scale), px + scale, py + scale, false);
    }
}
else if (item == 6)
{
    draw_circle(px, py, 4 * scale, false);
    draw_set_color(make_color_rgb(78, 121, 53));
    draw_rectangle(px, py - (6 * scale), px + scale, py - (3 * scale), false);
}
else if (item == 7)
{
    draw_set_color(make_color_rgb(116, 75, 43));
    draw_rectangle(px - scale, py, px + scale, py + (6 * scale), false);
    draw_set_color(make_color_rgb(255, 190, 55));
    draw_rectangle(px - (2 * scale), py - (5 * scale), px + (2 * scale), py, false);
}
draw_set_color(c_white);
return 1;
");

AddScript("scr_uc_draw_steve", @"
var sx = argument0;
var sy = argument1;
var walk = 0;
if (keyboard_check(vk_left) || keyboard_check(vk_right) || keyboard_check(vk_up) || keyboard_check(vk_down))
    walk = (floor(current_time / 120) mod 2);

// legs
if (walk == 0)
{
    draw_set_color(make_color_rgb(46, 55, 120));
    draw_rectangle(sx - 6, sy + 7, sx - 1, sy + 15, false);
    draw_rectangle(sx + 1, sy + 7, sx + 6, sy + 15, false);
}
else
{
    draw_set_color(make_color_rgb(46, 55, 120));
    draw_rectangle(sx - 7, sy + 7, sx - 2, sy + 14, false);
    draw_rectangle(sx + 2, sy + 8, sx + 7, sy + 16, false);
}
// shoes
draw_set_color(make_color_rgb(45, 36, 31));
draw_rectangle(sx - 6, sy + 14, sx - 1, sy + 16, false);
draw_rectangle(sx + 1, sy + 14, sx + 6, sy + 16, false);
// torso
draw_set_color(make_color_rgb(53, 169, 171));
draw_rectangle(sx - 7, sy - 5, sx + 7, sy + 8, false);
// arms / skin
draw_set_color(make_color_rgb(176, 125, 91));
draw_rectangle(sx - 10, sy - 4, sx - 7, sy + 7, false);
draw_rectangle(sx + 7, sy - 4, sx + 10, sy + 7, false);
// head
draw_set_color(make_color_rgb(177, 126, 92));
draw_rectangle(sx - 7, sy - 17, sx + 7, sy - 5, false);
// hair
draw_set_color(make_color_rgb(63, 42, 31));
draw_rectangle(sx - 7, sy - 17, sx + 7, sy - 14, false);
draw_rectangle(sx - 7, sy - 14, sx - 5, sy - 10, false);
// face
draw_set_color(make_color_rgb(65, 81, 104));
draw_rectangle(sx - 4, sy - 12, sx - 3, sy - 10, false);
draw_rectangle(sx + 3, sy - 12, sx + 4, sy - 10, false);
// held item
var held = global.uc_hotbar[global.uc_selected_slot];
var hx = sx + 13;
var hy = sy + 2;
if (global.uc_facing == 1) hx = sx - 13;
scr_uc_draw_item(held, hx, hy, 1);
draw_set_color(c_white);
return 1;
");

AddScript("scr_uc_draw_hotbar", @"
var ox = view_xview[0] + 188;
var oy = view_yview[0] + 438;
var i;
for (i = 0; i < 9; i += 1)
{
    var xx = ox + (i * 30);
    if (i == global.uc_selected_slot)
    {
        draw_set_color(c_white);
        draw_rectangle(xx - 2, oy - 2, xx + 26, oy + 26, true);
    }
    draw_set_color(make_color_rgb(37, 37, 37));
    draw_rectangle(xx, oy, xx + 24, oy + 24, false);
    draw_set_color(make_color_rgb(130, 130, 130));
    draw_rectangle(xx, oy, xx + 24, oy + 24, true);
    scr_uc_draw_item(global.uc_hotbar[i], xx + 12, oy + 12, 1);
    draw_set_color(c_white);
    draw_text(xx + 2, oy + 2, string(i + 1));
    if (global.uc_hotbar_count[i] > 1)
        draw_text(xx + 13, oy + 13, string(global.uc_hotbar_count[i]));
}
var held = global.uc_hotbar[global.uc_selected_slot];
draw_set_color(c_white);
draw_text(view_xview[0] + 188, view_yview[0] + 420, scr_uc_item_name(held));
return 1;
");

AddScript("scr_uc_use_selected", @"
var slot = global.uc_selected_slot;
var item = global.uc_hotbar[slot];
if ((item == 6) && (global.uc_hotbar_count[slot] > 0))
{
    if (global.hp < global.maxhp)
    {
        global.hp += 4;
        if (global.hp > global.maxhp) global.hp = global.maxhp;
        global.uc_hotbar_count[slot] -= 1;
        scr_uc_save();
        return 1;
    }
}
return 0;
");

AddScript("scr_uc_battle_draw", @"
// Small battle-state bridge: same selected slot and item are rendered in combat.
var bx = 565;
var by = 365;
draw_set_color(make_color_rgb(53, 169, 171));
draw_rectangle(bx - 7, by - 5, bx + 7, by + 8, false);
draw_set_color(make_color_rgb(177, 126, 92));
draw_rectangle(bx - 7, by - 17, bx + 7, by - 5, false);
draw_set_color(make_color_rgb(46, 55, 120));
draw_rectangle(bx - 6, by + 8, bx - 1, by + 16, false);
draw_rectangle(bx + 1, by + 8, bx + 6, by + 16, false);
scr_uc_draw_item(global.uc_hotbar[global.uc_selected_slot], bx + 13, by + 2, 1);
draw_set_color(c_white);
draw_text(490, 390, scr_uc_item_name(global.uc_hotbar[global.uc_selected_slot]));
return 1;
");

// Overworld integration.
importGroup.QueueAppend(mainchara.EventHandlerFor(EventType.Create, Data), @"
scr_uc_init();
scr_uc_spawn_room_blocks();
");

importGroup.QueueAppend(mainchara.EventHandlerFor(EventType.Step, EventSubtypeStep.Step, Data), @"
scr_uc_init();
image_alpha = 0;
if (keyboard_check(vk_down)) global.uc_facing = 0;
if (keyboard_check(vk_left)) global.uc_facing = 1;
if (keyboard_check(vk_right)) global.uc_facing = 2;
if (keyboard_check(vk_up)) global.uc_facing = 3;
if (keyboard_check_pressed(ord(""1""))) global.uc_selected_slot = 0;
if (keyboard_check_pressed(ord(""2""))) global.uc_selected_slot = 1;
if (keyboard_check_pressed(ord(""3""))) global.uc_selected_slot = 2;
if (keyboard_check_pressed(ord(""4""))) global.uc_selected_slot = 3;
if (keyboard_check_pressed(ord(""5""))) global.uc_selected_slot = 4;
if (keyboard_check_pressed(ord(""6""))) global.uc_selected_slot = 5;
if (keyboard_check_pressed(ord(""7""))) global.uc_selected_slot = 6;
if (keyboard_check_pressed(ord(""8""))) global.uc_selected_slot = 7;
if (keyboard_check_pressed(ord(""9""))) global.uc_selected_slot = 8;
if (keyboard_check_pressed(ord(""C""))) scr_uc_place_block();
if (keyboard_check_pressed(ord(""V""))) scr_uc_break_block();
if (keyboard_check_pressed(ord(""X""))) scr_uc_use_selected();
if (keyboard_check_pressed(ord(""R""))) scr_uc_save();
");

importGroup.QueueAppend(mainchara.EventHandlerFor(EventType.Draw, Data), @"
scr_uc_draw_steve(x, y);
scr_uc_draw_hotbar();
");

// Battle integration: selection remains shared and visible during fights.
importGroup.QueueAppend(battlecontroller.EventHandlerFor(EventType.Step, EventSubtypeStep.Step, Data), @"
scr_uc_init();
if (keyboard_check_pressed(ord(""1""))) global.uc_selected_slot = 0;
if (keyboard_check_pressed(ord(""2""))) global.uc_selected_slot = 1;
if (keyboard_check_pressed(ord(""3""))) global.uc_selected_slot = 2;
if (keyboard_check_pressed(ord(""4""))) global.uc_selected_slot = 3;
if (keyboard_check_pressed(ord(""5""))) global.uc_selected_slot = 4;
if (keyboard_check_pressed(ord(""6""))) global.uc_selected_slot = 5;
if (keyboard_check_pressed(ord(""7""))) global.uc_selected_slot = 6;
if (keyboard_check_pressed(ord(""8""))) global.uc_selected_slot = 7;
if (keyboard_check_pressed(ord(""9""))) global.uc_selected_slot = 8;
if (keyboard_check_pressed(ord(""X""))) scr_uc_use_selected();
");

importGroup.QueueAppend(battlecontroller.EventHandlerFor(EventType.Draw, Data), @"
scr_uc_battle_draw();
");

importGroup.Import();

ScriptMessage("Undercraft bootstrap installed. Controls: 1-9 select hotbar, C places a selected block, V breaks the targeted Undercraft block, X uses the selected item, R saves Undercraft state.");
