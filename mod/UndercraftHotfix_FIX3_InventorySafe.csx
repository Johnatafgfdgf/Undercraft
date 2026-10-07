// Undercraft FIX3: isolate inventory/crafting on bytecode 15.
// This hotfix keeps the core playable while the inventory backend is rebuilt
// without legacy-unsafe global array indexing.
EnsureDataLoaded();

UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data)
{
    MainThreadAction = MainThreadAction
};

void Replace(string name, string gml)
{
    var code = Data.Code.ByName(name);
    if (code != null)
        importGroup.QueueReplace(code, gml);
}

Replace("gml_Script_scr_uc_inventory_init", @"
if (!variable_global_exists(""uc_inventory_ready"")) global.uc_inventory_ready = 1;
global.uc_inventory_open = 0;
global.uc_crafting_open = 0;
global.uc_cursor = 0;
global.uc_recipe = 0;
if (!variable_global_exists(""uc_armor_head"")) global.uc_armor_head = 9;
if (!variable_global_exists(""uc_armor_chest"")) global.uc_armor_chest = 9;
if (!variable_global_exists(""uc_armor_legs"")) global.uc_armor_legs = 9;
if (!variable_global_exists(""uc_armor_boots"")) global.uc_armor_boots = 9;
return 1;
");

Replace("gml_Script_scr_uc_inventory_count", @"return 0;");
Replace("gml_Script_scr_uc_inventory_add", @"return 0;");
Replace("gml_Script_scr_uc_inventory_remove", @"return 0;");
Replace("gml_Script_scr_uc_swap_bag", @"return 0;");
Replace("gml_Script_scr_uc_can_craft", @"return 0;");
Replace("gml_Script_scr_uc_craft", @"return 0;");
Replace("gml_Script_scr_uc_equip", @"return 0;");
Replace("gml_Script_scr_uc_draw_inventory", @"return 0;");
Replace("gml_Script_scr_uc_draw_crafting", @"return 0;");

var step = Data.Code.ByName("gml_Object_obj_mainchara_Step_0");
if (step != null)
{
    importGroup.QueuePrepend(step, @"
global.uc_inventory_open = 0;
global.uc_crafting_open = 0;
");
}

importGroup.Import();
ScriptMessage("Undercraft FIX3 applied: inventory/crafting isolated for bytecode-15 core testing.");
