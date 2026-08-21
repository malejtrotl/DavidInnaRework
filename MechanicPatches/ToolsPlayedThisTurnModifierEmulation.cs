using HarmonyLib;
using Rift;

namespace DavidInnaRework.MechanicPatches;

// Emulates a "ScalePerToolPlayed" modifier without adding a new enum value.
//
// Convention for opting in:
//   _Modifiers       = EffectModifiers.ScalePerStrikePlayed
//   _ConditionEffect = AppliedEffectType.COUNT   (marker/sentinel)
//
// For effects that match that pattern, GetFinalValue is overridden to:
//   (int)perToolValue * toolsPlayedThisTurn
//
// toolsPlayedThisTurn is tracked from CombatManager.UseCard and reset at
// CombatManager.StartPlayerTurn.
public static class ToolsPlayedThisTurnModifierEmulationState
{
    internal static int ToolsPlayedThisTurn;
}

[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.StartPlayerTurn))]
public static class ToolsPlayedThisTurnModifierResetPatch
{
    static void Prefix()
    {
        ToolsPlayedThisTurnModifierEmulationState.ToolsPlayedThisTurn = 0;
    }
}

[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.UseCard))]
public static class ToolsPlayedThisTurnModifierTrackUseCardPatch
{
    static void Prefix(Card card)
    {
        if (card == null) return;

        var cardData = card.Data;
        if (cardData == null || cardData._CardType != CardType.Tool) return;

        ToolsPlayedThisTurnModifierEmulationState.ToolsPlayedThisTurn++;
    }
}

[HarmonyPatch(typeof(CardEffect), nameof(CardEffect.GetFinalValue))]
public static class ToolsPlayedThisTurnModifierGetFinalValuePatch
{
    // Marker for "treat ScalePerStrikePlayed as ScalePerToolPlayed".
    private const AppliedEffectType ToolPlayedMarker = AppliedEffectType.COUNT;

    static void Postfix(CardEffect __instance, Card card, ref float __result)
    {
        if (__instance == null || card == null) return;
        if (__instance._Modifiers != EffectModifiers.ScalePerStrikePlayed) return;
        if (__instance._ConditionEffect != ToolPlayedMarker) return;

        var toolsPlayed = ToolsPlayedThisTurnModifierEmulationState.ToolsPlayedThisTurn;
        if (toolsPlayed <= 0)
        {
            __result = 0;
            return;
        }

        var perToolValue = card.IsUpgraded ? __instance._EffectValueUpgraded : __instance._EffectValue;
        __result = perToolValue * toolsPlayed;
    }
}
