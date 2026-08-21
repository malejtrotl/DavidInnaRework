using System.Collections.Generic;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Rift;

namespace DavidInnaRework.MechanicPatches;

// Emulates a "ScalePerToolPlayed" modifier without adding a new enum value.
//
// Convention for opting in:
//   _Modifiers       = EffectModifiers.ScalePerStrikePlayed
//   _ConditionEffect = AppliedEffectType.COUNT   (marker/sentinel)
//
// For effects that match that pattern, GetFinalValue is overridden to:
//   perToolValue * toolsPlayedThisTurnByOwner
//
// toolsPlayedThisTurnByOwner is tracked from CombatManager.UseCard and reset
// at CombatManager.StartPlayerTurn.
public static class ToolsPlayedThisTurnModifierEmulationState
{
    internal static readonly Dictionary<System.IntPtr, int> ToolsPlayedThisTurnByOwner = new();
}

[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.StartPlayerTurn))]
public static class ToolsPlayedThisTurnResetPatch
{
    static void Prefix()
    {
        ToolsPlayedThisTurnModifierEmulationState.ToolsPlayedThisTurnByOwner.Clear();
    }
}

[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.UseCard))]
public static class ToolsPlayedThisTurnTrackUseCardPatch
{
    static void Prefix(Card card, Entity castingEntity)
    {
        if (card == null || castingEntity == null) return;

        var cardData = card.Data;
        if (cardData == null || cardData._CardType != CardType.Tool) return;

        var ownerPtr = IL2CPP.Il2CppObjectBaseToPtrNotNull(castingEntity);
        if (ownerPtr == System.IntPtr.Zero) return;

        if (!ToolsPlayedThisTurnModifierEmulationState.ToolsPlayedThisTurnByOwner.TryAdd(ownerPtr, 1))
        {
            ToolsPlayedThisTurnModifierEmulationState.ToolsPlayedThisTurnByOwner[ownerPtr]++;
        }
    }
}

[HarmonyPatch(typeof(CardEffect), nameof(CardEffect.GetFinalValue))]
public static class ToolsPlayedThisTurnGetFinalValuePatch
{
    // Marker for "treat ScalePerStrikePlayed as ScalePerToolPlayed".
    private const AppliedEffectType ToolsPlayedThisTurnMarker = AppliedEffectType.COUNT;

    static void Postfix(CardEffect __instance, Card card, ref float __result)
    {
        if (__instance == null) return;
        if (__instance._Modifiers != EffectModifiers.ScalePerStrikePlayed) return;
        if (__instance._ConditionEffect != ToolsPlayedThisTurnMarker) return;

        var owner = card?.Owner;
        if (owner == null)
        {
            __result = 0f;
            return;
        }

        var ownerPtr = IL2CPP.Il2CppObjectBaseToPtrNotNull(owner);
        if (ownerPtr == System.IntPtr.Zero
            || !ToolsPlayedThisTurnModifierEmulationState.ToolsPlayedThisTurnByOwner.TryGetValue(ownerPtr, out var toolsPlayed))
        {
            __result = 0f;
            return;
        }

        var perToolValue = card.IsUpgraded ? __instance._EffectValueUpgraded : __instance._EffectValue;
        __result = perToolValue * toolsPlayed;
    }
}
