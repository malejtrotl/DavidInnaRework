using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using DavidInnaRework.CardPatches;

namespace DavidInnaRework;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        // Card 0052 "Block" (see CardPatches/Card0052_Block.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(ShieldCardBuffPatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(BlockGrantsToughPatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(BlockDescriptionPatch));

        // Card 1003 "Ice Blast" (see CardPatches/Card1003_IceBlast.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(IceBlastPatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(IceBlastDescriptionPatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(IceBlastNamePatch));

        // Card 1400 "Improvise" (see CardPatches/Card1400_Improvise.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(ImproviseToolCountPatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(ImproviseLoseManaThenCreateToolsPatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(ImproviseDescriptionPatch));

        // Card 1418 "Cleansing Balm" (see CardPatches/Card1418_CleansingBalm.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(CleansingBalmCleanseCountPatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(CleansingBalmDescriptionPatch));

        // Card 1422 "Fire Bomb" (see CardPatches/Card1422_FireBomb.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(FireBombHitCountPatch));

        // Card 1423 "Caltrops" (see CardPatches/Card1423_Caltrops.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(CaltropsHitCountPatch));

        // Card 1432 "Bottled Ectoplasm" (see CardPatches/Card1432_BottledEctoplasm.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(BottledEctoplasmTriggersCursePatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(BottledEctoplasmDescriptionPatch));
    }
}
