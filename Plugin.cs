using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using DavidInnaRework.CardPatches;
using DavidInnaRework.MechanicPatches;

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

        // Tool-played modifier emulation (see MechanicPatches/ToolPlayedThisTurnModifierEmulation.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(ToolsPlayedThisTurnResetPatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(ToolsPlayedThisTurnTrackUseCardPatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(ToolsPlayedThisTurnGetFinalValuePatch));

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

        // Card 1403 "Sharpening Strike" (see CardPatches/Card1403_SharpeningStrike.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(SharpeningStrikeIncreaseDamagePatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(SharpeningStrikeDescriptionPatch));

        // Card 1407 "Ingenuity" (see CardPatches/Card1407_Ingenuity.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(IngenuityToolCountPatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(IngenuityCostAndDescriptionPatch));

        // Card 1408 "Frantic Scouring" (see CardPatches/Card1408_FranticScouring.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(FranticScouringDiscardCreatesToolsPatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(FranticScouringDescriptionPatch));

        // Card 1409 "Investigate" (see CardPatches/Card1409_Investigate.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(InvestigateToolCountPatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(InvestigateDescriptionPatch));

        // Card 1414 "Adventurer's Log" (see CardPatches/Card1414_AdventurersLog.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(AdventurersLogUpgradedDrawPatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(AdventurersLogUpgradedCostPatch));

        // Card 1411 "Resourceful Strike" (see CardPatches/Card1411_ResourcefulStrike.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(ResourcefulStrikeDamagePatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(ResourcefulStrikeOtherEffectPatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(ResourcefulStrikeDescriptionPatch));

        // Card 1418 "Cleansing Balm" (see CardPatches/Card1418_CleansingBalm.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(CleansingBalmCleanseCountPatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(CleansingBalmDescriptionPatch));

        // Card 1422 "Fire Bomb" (see CardPatches/Card1422_FireBomb.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(FireBombHitCountPatch));

        // Card 1423 "Caltrops" (see CardPatches/Card1423_Caltrops.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(CaltropsHitCountPatch));

        // Card 1426 "Unstable Darkstone" (see CardPatches/Card1426_UnstableDarkstone.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(UnstableDarkstoneDispelPatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(UnstableDarkstoneDescriptionPatch));

        // Card 1427 "Inkwell and Quill" (see CardPatches/Card1427_InkwellAndQuill.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(InkwellAndQuillUpgradablePatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(InkwellAndQuillUpgradedValuePatch));

        // Card 1432 "Bottled Ectoplasm" (see CardPatches/Card1432_BottledEctoplasm.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(BottledEctoplasmTriggersCursePatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(BottledEctoplasmDescriptionPatch));
    }
}
