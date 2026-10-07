// Undercraft FIX5 - unconditional core bootstrap for Undertale bytecode 15
EnsureDataLoaded();
var importGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data) { MainThreadAction = MainThreadAction };

var create = Data.Code.ByName("gml_Object_obj_mainchara_Create_0");
var init = Data.Code.ByName("gml_Script_scr_uc_init");
var save = Data.Code.ByName("gml_Script_scr_uc_save");
var load = Data.Code.ByName("gml_Script_scr_uc_load");
var spawn = Data.Code.ByName("gml_Script_scr_uc_spawn_blocks");
var inv = Data.Code.ByName("gml_Script_scr_uc_inventory_init");
var draw = Data.Code.ByName("gml_Object_obj_mainchara_Draw_0");

if (create == null || init == null || save == null || load == null || draw == null)
{
    ScriptError("Undercraft FIX5: required code entry missing.");
    return;
}

importGroup.QueuePrepend(create, @"
global.uc_initialized = 1;
global.uc_slot = 0;
global.uc_facing = 0;
global.uc_world_count = 0;
global.uc_inventory_open = 0;
global.uc_crafting_open = 0;
global.uc_inventory_ready = 0;
global.uc_cursor = 0;
global.uc_recipe = 0;
global.uc_armor_head = 0;
global.uc_armor_chest = 0;
global.uc_armor_legs = 0;
global.uc_armor_boots = 0;

global.uc_hotbar[0] = 1;
global.uc_hotbar[1] = 2;
global.uc_hotbar[2] = 3;
global.uc_hotbar[3] = 4;
global.uc_hotbar[4] = 5;
global.uc_hotbar[5] = 6;
global.uc_hotbar[6] = 7;
global.uc_hotbar[7] = 8;
global.uc_hotbar[8] = 0;

global.uc_count[0] = 32;
global.uc_count[1] = 32;
global.uc_count[2] = 32;
global.uc_count[3] = 1;
global.uc_count[4] = 1;
global.uc_count[5] = 3;
global.uc_count[6] = 16;
global.uc_count[7] = 1;
global.uc_count[8] = 0;
");

importGroup.QueueReplace(init, @"return 1;");
importGroup.QueueReplace(save, @"return 1;");
importGroup.QueueReplace(load, @"return 1;");
if (spawn != null) importGroup.QueueReplace(spawn, @"return 1;");
if (inv != null) importGroup.QueueReplace(inv, @"return 1;");
importGroup.QueueFindReplace(draw, "UNDERCRAFT FIX4", "UNDERCRAFT FIX5");
importGroup.Import();
ScriptMessage("Undercraft FIX5 installed: unconditional core bootstrap, persistence disabled.");
