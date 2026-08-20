using HarmonyLib;
using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1414 "Adventurer's Log": the UPGRADED version now costs 2 and draws 2
// cards. The unupgraded version is left exactly as-is, so only the *Upgraded
// fields are written.
//
// Draw count is a Prefix on CardEffect.GetFinalValue, mutating
// _EffectValueUpgraded directly (rather than the return value) so every
// downstream system reads the new number and no "modified" highlight is
// triggered. Guarded on _Mode so only the Draw effect is touched —
// GetFinalValue fires once per effect, and the card may have others.
[HarmonyPatch(typeof(CardEffect), nameof(CardEffect.GetFinalValue))]
public static class AdventurersLogUpgradedDrawPatch
{
    private const int AdventurersLogCardId = 1414;
    private const int UpgradedDrawCount = 2;

    static void Prefix(CardEffect __instance)
    {
        var cardData = __instance.CardData;
        if (cardData == null || cardData._CardID != AdventurersLogCardId) return;
        if (__instance._Mode != EffectMode.Draw) return;

        __instance._EffectValueUpgraded = UpgradedDrawCount;
    }
}

// Sets the upgraded mana cost. _CostUpgraded is a plain writable field on
// CardData, set from a Prefix on GetDescription (fires once per card and
// reliably whenever the card needs displaying). _Cost is deliberately left
// untouched so the unupgraded version keeps its original cost.
[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class AdventurersLogUpgradedCostPatch
{
    private const int AdventurersLogCardId = 1414;
    private const int UpgradedCost = 2;

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != AdventurersLogCardId) return;

        __instance._CostUpgraded = UpgradedCost;
    }
}
