// Undercraft v0.1 hotfix: initialize world state before block spawning.
EnsureDataLoaded();

var spawn = Data.Code.ByName("gml_Script_scr_uc_spawn_blocks");
var create = Data.Code.ByName("gml_Object_obj_mainchara_Create_0");

if (spawn == null || create == null)
{
    ScriptError("Undercraft hotfix: required code entries were not found.");
    return;
}

UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data)
{
    MainThreadAction = MainThreadAction
};

importGroup.QueuePrepend(spawn, @"
if (!variable_global_exists(""uc_world_count""))
{
    global.uc_world_count = 0;
}
");

importGroup.QueuePrepend(create, @"
if (!variable_global_exists(""uc_world_count""))
{
    global.uc_world_count = 0;
}
scr_uc_init();
");

importGroup.Import();
ScriptMessage("Undercraft hotfix installed: world state is initialized before block spawning.");
