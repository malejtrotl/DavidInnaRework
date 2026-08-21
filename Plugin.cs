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

        // Configure generic trigger targets (see
        // MechanicPatches/NoFatigueDrawOnToolPlayed.cs) BEFORE the game-load
        // initializer below runs, so it knows which card to flag. Draw
        // Improvised Strike (Card ID 1401) whenever a Tool is played.
        NoFatigueDrawOnToolPlayedState.Configure(1401);

        // One-time card mutation initializer (effects/numeric data AND text),
        // applied once at real game-load time (see
        // MechanicPatches/CardDataGameLoadInitializer.cs). Covers every card
        // listed there: 1400, 1401, 1403, 1407, 1408, 1409, 1411, 1414, 1418,
        // 1422, 1423, 1426, 1427, 1432, plus the NoFatigueDrawOnToolPlayed
        // flag above.
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(CardDataGameLoadInitializerPatch));

        // Tool-played modifier emulation (see MechanicPatches/ToolsPlayedThisTurnModifierEmulation.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(ToolsPlayedThisTurnModifierResetPatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(ToolsPlayedThisTurnModifierTrackUseCardPatch));
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(ToolsPlayedThisTurnModifierGetFinalValuePatch));

        // No-fatigue draw on Tool played mechanic (see MechanicPatches/NoFatigueDrawOnToolPlayed.cs)
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(typeof(NoFatigueDrawOnToolPlayedPatch));
    }
}
