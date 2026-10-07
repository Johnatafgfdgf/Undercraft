// Undercraft FIX2 boot recovery.
// Temporary: disables persisted block restoration so the game can boot even if
// legacy state globals have not been initialized yet.
EnsureDataLoaded();

var create = Data.Code.ByName("gml_Object_obj_mainchara_Create_0");
var spawn = Data.Code.ByName("gml_Script_scr_uc_spawn_blocks");

if (create == null || spawn == null)
{
    ScriptError("Undercraft FIX2: required code entries not found.");
    return;
}

UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data)
{
    MainThreadAction = MainThreadAction
};

importGroup.QueuePrepend(create, @"
global.uc_world_count = 0;
");

importGroup.QueueReplace(spawn, @"
if (!variable_global_exists(""uc_world_count""))
{
    global.uc_world_count = 0;
}
return 1;
");

importGroup.Import();
ScriptMessage("Undercraft FIX2 installed: block restoration disabled for boot validation.");
