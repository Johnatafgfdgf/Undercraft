// Undercraft gameplay expansion
// Apply AFTER UndercraftBootstrap.csx.

EnsureDataLoaded();

if (Data.Scripts.ByName("scr_uc_init") == null)
{
    ScriptError("UndercraftGameplay: bootstrap must be applied first.");
    return;
}

var mainchara = Data.GameObjects.ByName("obj_mainchara");
var battlecontroller = Data.GameObjects.ByName("obj_battlecontroller");
if (mainchara == null || battlecontroller == null)
{
    ScriptError("UndercraftGameplay: expected UNDERTALE objects were not found.");
    return;
}

void AddScript(string name, string gml)
{
    var old = Data.Scripts.ByName(name);
    if (old != null)
        return;

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

string[] ucFunctions = new string[]
{
    "scr_uc_inv_init",
    "scr_uc_inventory_count",
    "scr_uc_inventory_add",
    "scr_uc_inventory_remove",
    "scr_uc_swap_bag_hotbar",
    "scr_uc_can_craft",
    "scr_uc_craft",
    "scr_uc_find_crafting_table",
    "scr_uc_draw_inventory",
    "scr_uc_draw_crafting",
    "scr_uc_draw_armor",
    "scr_uc_equip_cursor_item",
    "scr_uc_armor_defense"
};
foreach (string fn in ucFunctions)
    Data.Functions.EnsureDefined(fn, Data.Strings);

AddScript("scr_uc_inv_init", @"
if (!variable_global_exists(""uc_inv_initialized""))
{
    global.uc_inv_initialized = 1;
    global.uc_inventory_open = 0;
    global.uc_crafting_open = 0;
    global.uc_inv_cursor = 0;
    global.uc_recipe_cursor = 0;
    global.uc_armor_head = 9;
    global.uc_armor_chest = 9;
    global.uc_armor_legs = 9;
    global.uc_armor_boots = 9;

    var i;
    for (i = 0; i < 27; i += 1)
    {
        global.uc_bag_item[i] = 9;
        global.uc_bag_count[i] = 0;
    }

    // Development starter resources. These are intentionally small and will
    // later be replaced by world drops/progression.
    global.uc_bag_item[0] = 10;
    global.uc_bag_count[0] = 8;
    global.uc_bag_item[1] = 14;
    global.uc_bag_count[1] = 1;
    global.uc_bag_item[2] = 15;
    global.uc_bag_count[2] = 1;
    global.uc_bag_item[3] = 16;
    global.uc_bag_count[3] = 1;
    global.uc_bag_item[4] = 17;
    global.uc_bag_count[4] = 1;
}
return 1;
");

AddScript("scr_uc_inventory_count", @"
var item = argument0;
var total = 0;
var i;
for (i = 0; i < 9; i += 1)
    if (global.uc_hotbar[i] == item) total += global.uc_hotbar_count[i];
for (i = 0; i < 27; i += 1)
    if (global.uc_bag_item[i] == item) total += global.uc_bag_count[i];
return total;
");

AddScript("scr_uc_inventory_add", @"
var item = argument0;
var amount = argument1;
if (amount <= 0) return 1;
var i;

// Prefer an existing hotbar stack.
for (i = 0; i < 9; i += 1)
{
    if ((global.uc_hotbar[i] == item) && (global.uc_hotbar_count[i] > 0))
    {
        global.uc_hotbar_count[i] += amount;
        return 1;
    }
}
// Then an existing bag stack.
for (i = 0; i < 27; i += 1)
{
    if ((global.uc_bag_item[i] == item) && (global.uc_bag_count[i] > 0))
    {
        global.uc_bag_count[i] += amount;
        return 1;
    }
}
// Finally an empty bag slot.
for (i = 0; i < 27; i += 1)
{
    if ((global.uc_bag_count[i] <= 0) || (global.uc_bag_item[i] == 9))
    {
        global.uc_bag_item[i] = item;
        global.uc_bag_count[i] = amount;
        return 1;
    }
}
return 0;
");

AddScript("scr_uc_inventory_remove", @"
var item = argument0;
var amount = argument1;
if (amount <= 0) return 1;
if (scr_uc_inventory_count(item) < amount) return 0;

var i;
for (i = 0; i < 9; i += 1)
{
    if ((global.uc_hotbar[i] == item) && (amount > 0))
    {
        var take = min(global.uc_hotbar_count[i], amount);
        global.uc_hotbar_count[i] -= take;
        amount -= take;
        if (global.uc_hotbar_count[i] <= 0)
        {
            global.uc_hotbar_count[i] = 0;
            global.uc_hotbar[i] = 9;
        }
    }
}
for (i = 0; i < 27; i += 1)
{
    if ((global.uc_bag_item[i] == item) && (amount > 0))
    {
        var take2 = min(global.uc_bag_count[i], amount);
        global.uc_bag_count[i] -= take2;
        amount -= take2;
        if (global.uc_bag_count[i] <= 0)
        {
            global.uc_bag_count[i] = 0;
            global.uc_bag_item[i] = 9;
        }
    }
}
return (amount <= 0);
");

AddScript("scr_uc_swap_bag_hotbar", @"
var bslot = argument0;
if ((bslot < 0) || (bslot >= 27)) return 0;
var hslot = global.uc_selected_slot;

var ti = global.uc_hotbar[hslot];
var tc = global.uc_hotbar_count[hslot];
global.uc_hotbar[hslot] = global.uc_bag_item[bslot];
global.uc_hotbar_count[hslot] = global.uc_bag_count[bslot];
global.uc_bag_item[bslot] = ti;
global.uc_bag_count[bslot] = tc;

// Fresh durability when a wooden tool enters a hotbar slot for the first time.
if (global.uc_hotbar[hslot] == 4) global.uc_hotbar_durability[hslot] = 59;
if (global.uc_hotbar[hslot] == 5) global.uc_hotbar_durability[hslot] = 59;

scr_uc_save();
return 1;
");

AddScript("scr_uc_can_craft", @"
var recipe = argument0;
if (recipe == 0) return (scr_uc_inventory_count(10) >= 1);
if (recipe == 1) return (scr_uc_inventory_count(11) >= 2);
if (recipe == 2) return (scr_uc_inventory_count(11) >= 4);
if (recipe == 3) return ((scr_uc_inventory_count(11) >= 3) && (scr_uc_inventory_count(12) >= 2));
if (recipe == 4) return ((scr_uc_inventory_count(11) >= 2) && (scr_uc_inventory_count(12) >= 1));
return 0;
");

AddScript("scr_uc_craft", @"
var recipe = argument0;
if (!scr_uc_can_craft(recipe)) return 0;

if (recipe == 0)
{
    scr_uc_inventory_remove(10, 1);
    scr_uc_inventory_add(11, 4);
}
else if (recipe == 1)
{
    scr_uc_inventory_remove(11, 2);
    scr_uc_inventory_add(12, 4);
}
else if (recipe == 2)
{
    scr_uc_inventory_remove(11, 4);
    scr_uc_inventory_add(8, 1);
}
else if (recipe == 3)
{
    scr_uc_inventory_remove(11, 3);
    scr_uc_inventory_remove(12, 2);
    scr_uc_inventory_add(4, 1);
}
else if (recipe == 4)
{
    scr_uc_inventory_remove(11, 2);
    scr_uc_inventory_remove(12, 1);
    scr_uc_inventory_add(5, 1);
}

scr_uc_save();
return 1;
");

AddScript("scr_uc_find_crafting_table", @"
var total = instance_number(obj_uc_block);
var i;
for (i = 0; i < total; i += 1)
{
    var b = instance_find(obj_uc_block, i);
    if ((b != noone) && (b.uc_type == 8))
    {
        if (point_distance(obj_mainchara.x, obj_mainchara.y, b.x, b.y) <= 38)
            return b;
    }
}
return noone;
");

AddScript("scr_uc_armor_defense", @"
var value = 0;
if (global.uc_armor_head == 14) value += 1;
if (global.uc_armor_chest == 15) value += 3;
if (global.uc_armor_legs == 16) value += 2;
if (global.uc_armor_boots == 17) value += 1;
return value;
");

AddScript("scr_uc_equip_cursor_item", @"
var slot = global.uc_inv_cursor;
var item = global.uc_bag_item[slot];
if (global.uc_bag_count[slot] <= 0) return 0;

var old = 9;
if (item == 14)
{
    old = global.uc_armor_head;
    global.uc_armor_head = item;
}
else if (item == 15)
{
    old = global.uc_armor_chest;
    global.uc_armor_chest = item;
}
else if (item == 16)
{
    old = global.uc_armor_legs;
    global.uc_armor_legs = item;
}
else if (item == 17)
{
    old = global.uc_armor_boots;
    global.uc_armor_boots = item;
}
else return 0;

global.uc_bag_count[slot] -= 1;
if (global.uc_bag_count[slot] <= 0)
{
    global.uc_bag_count[slot] = 0;
    global.uc_bag_item[slot] = 9;
}
if (old != 9) scr_uc_inventory_add(old, 1);
scr_uc_save();
return 1;
");

AddScript("scr_uc_draw_armor", @"
var sx = argument0;
var sy = argument1;

// Helmet
if (global.uc_armor_head == 14)
{
    draw_set_color(make_color_rgb(126, 88, 58));
    draw_rectangle(sx - 8, sy - 18, sx + 8, sy - 13, false);
    draw_rectangle(sx - 8, sy - 13, sx - 6, sy - 8, false);
    draw_rectangle(sx + 6, sy - 13, sx + 8, sy - 8, false);
}
// Chestplate
if (global.uc_armor_chest == 15)
{
    draw_set_color(make_color_rgb(139, 95, 61));
    draw_rectangle(sx - 8, sy - 6, sx + 8, sy + 5, true);
    draw_rectangle(sx - 10, sy - 5, sx - 8, sy + 5, false);
    draw_rectangle(sx + 8, sy - 5, sx + 10, sy + 5, false);
}
// Leggings
if (global.uc_armor_legs == 16)
{
    draw_set_color(make_color_rgb(121, 82, 54));
    draw_rectangle(sx - 7, sy + 6, sx - 1, sy + 14, true);
    draw_rectangle(sx + 1, sy + 6, sx + 7, sy + 14, true);
}
// Boots
if (global.uc_armor_boots == 17)
{
    draw_set_color(make_color_rgb(102, 68, 47));
    draw_rectangle(sx - 7, sy + 13, sx - 1, sy + 17, false);
    draw_rectangle(sx + 1, sy + 13, sx + 7, sy + 17, false);
}
draw_set_color(c_white);
return 1;
");

AddScript("scr_uc_draw_inventory", @"
if (!global.uc_inventory_open) return 0;

var ox = view_xview[0] + 170;
var oy = view_yview[0] + 100;
draw_set_alpha(0.94);
draw_set_color(make_color_rgb(24, 24, 24));
draw_rectangle(ox, oy, ox + 300, oy + 190, false);
draw_set_alpha(1);
draw_set_color(c_white);
draw_rectangle(ox, oy, ox + 300, oy + 190, true);
draw_text(ox + 12, oy + 10, ""UNDERCRAFT INVENTORY"");

var i;
for (i = 0; i < 27; i += 1)
{
    var col = i mod 9;
    var row = floor(i / 9);
    var xx = ox + 12 + (col * 30);
    var yy = oy + 38 + (row * 30);

    draw_set_color(make_color_rgb(55, 55, 55));
    draw_rectangle(xx, yy, xx + 24, yy + 24, false);
    draw_set_color(make_color_rgb(130, 130, 130));
    draw_rectangle(xx, yy, xx + 24, yy + 24, true);

    if (i == global.uc_inv_cursor)
    {
        draw_set_color(c_white);
        draw_rectangle(xx - 2, yy - 2, xx + 26, yy + 26, true);
    }

    if (global.uc_bag_count[i] > 0)
    {
        scr_uc_draw_item(global.uc_bag_item[i], xx + 12, yy + 12, 1);
        draw_set_color(c_white);
        if (global.uc_bag_count[i] > 1)
            draw_text(xx + 13, yy + 13, string(global.uc_bag_count[i]));
    }
}

var selected = global.uc_bag_item[global.uc_inv_cursor];
draw_set_color(c_white);
draw_text(ox + 12, oy + 136, ""Selected: "" + scr_uc_item_name(selected));
draw_text(ox + 12, oy + 151, ""H: move to/from hand   Q: equip armor"");
draw_text(ox + 12, oy + 166, ""Armor DEF: "" + string(scr_uc_armor_defense()));
return 1;
");

AddScript("scr_uc_draw_crafting", @"
if (!global.uc_crafting_open) return 0;

var ox = view_xview[0] + 190;
var oy = view_yview[0] + 92;
draw_set_alpha(0.96);
draw_set_color(make_color_rgb(40, 27, 18));
draw_rectangle(ox, oy, ox + 260, oy + 210, false);
draw_set_alpha(1);
draw_set_color(c_white);
draw_rectangle(ox, oy, ox + 260, oy + 210, true);
draw_text(ox + 12, oy + 10, ""CRAFTING TABLE"");

var names0 = ""Oak Log -> 4 Oak Planks"";
var names1 = ""2 Planks -> 4 Sticks"";
var names2 = ""4 Planks -> Crafting Table"";
var names3 = ""3 Planks + 2 Sticks -> Pickaxe"";
var names4 = ""2 Planks + 1 Stick -> Sword"";

var i;
for (i = 0; i < 5; i += 1)
{
    var yy = oy + 40 + (i * 27);
    if (i == global.uc_recipe_cursor)
    {
        draw_set_color(make_color_rgb(92, 70, 42));
        draw_rectangle(ox + 8, yy - 3, ox + 252, yy + 18, false);
    }
    draw_set_color(scr_uc_can_craft(i) ? c_white : make_color_rgb(120, 120, 120));
    if (i == 0) draw_text(ox + 14, yy, names0);
    if (i == 1) draw_text(ox + 14, yy, names1);
    if (i == 2) draw_text(ox + 14, yy, names2);
    if (i == 3) draw_text(ox + 14, yy, names3);
    if (i == 4) draw_text(ox + 14, yy, names4);
}
draw_set_color(c_white);
draw_text(ox + 12, oy + 182, ""Z: craft   E: close"");
return 1;
");

UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data)
{
    MainThreadAction = MainThreadAction
};

// Extend item registry.
importGroup.QueueReplace(Data.Scripts.ByName("scr_uc_item_name").Code, @"
var item = argument0;
if (item == 1) return ""Grass Block"";
if (item == 2) return ""Dirt Block"";
if (item == 3) return ""Stone Block"";
if (item == 4) return ""Wooden Pickaxe"";
if (item == 5) return ""Wooden Sword"";
if (item == 6) return ""Apple"";
if (item == 7) return ""Torch"";
if (item == 8) return ""Crafting Table"";
if (item == 10) return ""Oak Log"";
if (item == 11) return ""Oak Planks"";
if (item == 12) return ""Stick"";
if (item == 13) return ""Cobblestone"";
if (item == 14) return ""Leather Helmet"";
if (item == 15) return ""Leather Chestplate"";
if (item == 16) return ""Leather Leggings"";
if (item == 17) return ""Leather Boots"";
return ""Empty"";
");

importGroup.QueueReplace(Data.Scripts.ByName("scr_uc_item_color").Code, @"
var item = argument0;
if (item == 1) return make_color_rgb(76, 153, 65);
if (item == 2) return make_color_rgb(121, 85, 58);
if (item == 3) return make_color_rgb(124, 124, 124);
if (item == 4) return make_color_rgb(170, 125, 72);
if (item == 5) return make_color_rgb(196, 150, 82);
if (item == 6) return make_color_rgb(204, 42, 42);
if (item == 7) return make_color_rgb(240, 177, 48);
if (item == 8) return make_color_rgb(157, 102, 54);
if (item == 10) return make_color_rgb(105, 75, 47);
if (item == 11) return make_color_rgb(184, 145, 87);
if (item == 12) return make_color_rgb(151, 108, 66);
if (item == 13) return make_color_rgb(104, 104, 104);
if ((item >= 14) && (item <= 17)) return make_color_rgb(139, 95, 61);
return c_black;
");

// Draw newly-added item types while preserving original rendering behavior.
importGroup.QueueReplace(Data.Scripts.ByName("scr_uc_draw_item").Code, @"
var item = argument0;
var px = argument1;
var py = argument2;
var scale = argument3;
var c = scr_uc_item_color(item);
draw_set_color(c);

if (scr_uc_item_is_block(item) || (item == 10) || (item == 11) || (item == 13))
{
    draw_rectangle(px - (4 * scale), py - (4 * scale), px + (4 * scale), py + (4 * scale), false);
    draw_set_color(c_black);
    draw_rectangle(px - (4 * scale), py - (4 * scale), px + (4 * scale), py + (4 * scale), true);
    if (item == 10)
    {
        draw_set_color(make_color_rgb(76, 52, 34));
        draw_line(px, py - (4 * scale), px, py + (4 * scale));
    }
    if (item == 11)
    {
        draw_set_color(make_color_rgb(125, 91, 55));
        draw_line(px - (4 * scale), py, px + (4 * scale), py);
    }
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
        draw_rectangle(px - scale, py - (8 * scale), px + scale, py + scale, false);
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
else if (item == 12)
{
    draw_set_color(c);
    draw_rectangle(px - scale, py - (5 * scale), px + scale, py + (5 * scale), false);
}
else if ((item >= 14) && (item <= 17))
{
    draw_set_color(c);
    draw_rectangle(px - (4 * scale), py - (3 * scale), px + (4 * scale), py + (4 * scale), true);
}
draw_set_color(c_white);
return 1;
");

// Extend save/load. The separate undercraft.ini keeps mod state namespaced.
importGroup.QueueReplace(Data.Scripts.ByName("scr_uc_save").Code, @"
scr_uc_inv_init();
ini_open(""undercraft.ini"");
ini_write_real(""player"", ""selected_slot"", global.uc_selected_slot);
ini_write_real(""player"", ""facing"", global.uc_facing);
ini_write_real(""player"", ""armor_head"", global.uc_armor_head);
ini_write_real(""player"", ""armor_chest"", global.uc_armor_chest);
ini_write_real(""player"", ""armor_legs"", global.uc_armor_legs);
ini_write_real(""player"", ""armor_boots"", global.uc_armor_boots);

var i;
for (i = 0; i < 9; i += 1)
{
    ini_write_real(""hotbar"", ""slot_"" + string(i), global.uc_hotbar[i]);
    ini_write_real(""hotbar"", ""count_"" + string(i), global.uc_hotbar_count[i]);
    ini_write_real(""hotbar"", ""durability_"" + string(i), global.uc_hotbar_durability[i]);
}
for (i = 0; i < 27; i += 1)
{
    ini_write_real(""bag"", ""slot_"" + string(i), global.uc_bag_item[i]);
    ini_write_real(""bag"", ""count_"" + string(i), global.uc_bag_count[i]);
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

importGroup.QueueReplace(Data.Scripts.ByName("scr_uc_load").Code, @"
scr_uc_inv_init();
ini_open(""undercraft.ini"");
global.uc_selected_slot = ini_read_real(""player"", ""selected_slot"", 0);
global.uc_facing = ini_read_real(""player"", ""facing"", 0);
global.uc_armor_head = ini_read_real(""player"", ""armor_head"", global.uc_armor_head);
global.uc_armor_chest = ini_read_real(""player"", ""armor_chest"", global.uc_armor_chest);
global.uc_armor_legs = ini_read_real(""player"", ""armor_legs"", global.uc_armor_legs);
global.uc_armor_boots = ini_read_real(""player"", ""armor_boots"", global.uc_armor_boots);

var i;
for (i = 0; i < 9; i += 1)
{
    global.uc_hotbar[i] = ini_read_real(""hotbar"", ""slot_"" + string(i), global.uc_hotbar[i]);
    global.uc_hotbar_count[i] = ini_read_real(""hotbar"", ""count_"" + string(i), global.uc_hotbar_count[i]);
    global.uc_hotbar_durability[i] = ini_read_real(""hotbar"", ""durability_"" + string(i), global.uc_hotbar_durability[i]);
}
for (i = 0; i < 27; i += 1)
{
    global.uc_bag_item[i] = ini_read_real(""bag"", ""slot_"" + string(i), global.uc_bag_item[i]);
    global.uc_bag_count[i] = ini_read_real(""bag"", ""count_"" + string(i), global.uc_bag_count[i]);
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

importGroup.QueueReplace(Data.Scripts.ByName("scr_uc_init").Code, @"
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

    var i;
    for (i = 0; i < 9; i += 1) global.uc_hotbar_durability[i] = 0;
    global.uc_hotbar_durability[3] = 59;
    global.uc_hotbar_durability[4] = 59;

    scr_uc_inv_init();
    scr_uc_load();
}
return 1;
");

// Add durability to mining while retaining persistent world delta behavior.
importGroup.QueueReplace(Data.Scripts.ByName("scr_uc_break_block").Code, @"
var tx = scr_uc_target_x();
var ty = scr_uc_target_y();
var b = instance_position(tx, ty, obj_uc_block);
if (b == noone) return 0;

var broken_type = b.uc_type;
var slot = global.uc_selected_slot;
var held_item = global.uc_hotbar[slot];
if ((broken_type == 3) && (held_item != 4)) return 0;

var i;
var found = -1;
for (i = 0; i < global.uc_block_count; i += 1)
{
    if ((global.uc_block_room[i] == room) &&
        (global.uc_block_x[i] == tx) &&
        (global.uc_block_y[i] == ty))
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

scr_uc_inventory_add(broken_type, 1);

if ((held_item == 4) && (broken_type == 3))
{
    global.uc_hotbar_durability[slot] -= 1;
    if (global.uc_hotbar_durability[slot] <= 0)
    {
        global.uc_hotbar[slot] = 9;
        global.uc_hotbar_count[slot] = 0;
        global.uc_hotbar_durability[slot] = 0;
    }
}

with (b) instance_destroy();
scr_uc_save();
return 1;
");

// Input bridge.
importGroup.QueueAppend(mainchara.EventHandlerFor(EventType.Step, EventSubtypeStep.Step, Data), @"
scr_uc_inv_init();

if (keyboard_check_pressed(ord(""I"")))
{
    global.uc_inventory_open = 1 - global.uc_inventory_open;
    if (global.uc_inventory_open) global.uc_crafting_open = 0;
}

if (keyboard_check_pressed(ord(""E"")))
{
    if (global.uc_crafting_open)
        global.uc_crafting_open = 0;
    else if (scr_uc_find_crafting_table() != noone)
    {
        global.uc_crafting_open = 1;
        global.uc_inventory_open = 0;
    }
}

if (global.uc_inventory_open)
{
    if (keyboard_check_pressed(vk_left)) global.uc_inv_cursor -= 1;
    if (keyboard_check_pressed(vk_right)) global.uc_inv_cursor += 1;
    if (keyboard_check_pressed(vk_up)) global.uc_inv_cursor -= 9;
    if (keyboard_check_pressed(vk_down)) global.uc_inv_cursor += 9;
    while (global.uc_inv_cursor < 0) global.uc_inv_cursor += 27;
    while (global.uc_inv_cursor >= 27) global.uc_inv_cursor -= 27;

    if (keyboard_check_pressed(ord(""H"")))
        scr_uc_swap_bag_hotbar(global.uc_inv_cursor);
    if (keyboard_check_pressed(ord(""Q"")))
        scr_uc_equip_cursor_item();
}

if (global.uc_crafting_open)
{
    if (scr_uc_find_crafting_table() == noone)
        global.uc_crafting_open = 0;
    else
    {
        if (keyboard_check_pressed(vk_up)) global.uc_recipe_cursor -= 1;
        if (keyboard_check_pressed(vk_down)) global.uc_recipe_cursor += 1;
        if (global.uc_recipe_cursor < 0) global.uc_recipe_cursor = 4;
        if (global.uc_recipe_cursor > 4) global.uc_recipe_cursor = 0;
        if (keyboard_check_pressed(ord(""Z"")))
            scr_uc_craft(global.uc_recipe_cursor);
    }
}
");

importGroup.QueueAppend(mainchara.EventHandlerFor(EventType.Draw, Data), @"
scr_uc_draw_armor(x, y);
scr_uc_draw_inventory();
scr_uc_draw_crafting();
");

// Armor state is also visible in combat.
importGroup.QueueAppend(battlecontroller.EventHandlerFor(EventType.Draw, Data), @"
scr_uc_draw_armor(565, 365);
");

importGroup.Import();

ScriptMessage("Undercraft gameplay expansion installed: inventory, crafting, armor and tool durability are active.");
