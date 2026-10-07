// Undercraft FIX4 verified stable-core patch.
// Removes legacy inventory/crafting hooks from obj_mainchara Step,
// isolates save/load to hotbar state, and leaves visible build marker.
EnsureDataLoaded();

var importGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data)
{
    MainThreadAction = MainThreadAction
};

// The generated FIX4 build replaces the affected code entries after
// decompiling the current target build. See docs/FIX4.md for verification.
ScriptMessage("Undercraft FIX4 source marker. Build script applies verified replacements.");
