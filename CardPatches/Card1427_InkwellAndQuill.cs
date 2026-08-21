using HarmonyLib;
using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1427 "Inkwell and Quill" is not upgradable by default. This makes it
// upgradable, with the upgraded version costing 2 and its single effect value
// also being 2.
//
// Upgradability is gated by the CardModifiers.NoUpgrade flag on
// CardData._Modifiers. CardModifiers is a [Flags] bitmask, so the flag is
// cleared with &= ~ rather than by overwriting _Modifiers wholesale — that
// preserves any other flags the card carries.
//
// _Modifiers and _CostUpgraded are plain writable fields, set from a Prefix on
// GetDescription (fires once per card and reliably whenever the card needs
// displaying).
[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class InkwellAndQuillUpgradablePatch
{
    private const int InkwellAndQuillCardId = 1427;
    private const int UpgradedCost = 2;

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != InkwellAndQuillCardId) return;

        // Clear the "cannot be upgraded" flag, leaving other flags intact.
        __instance._Modifiers &= ~CardModifiers.NoUpgrade;

        __instance._CostUpgraded = UpgradedCost;
    }
}

// Sets the upgraded value of the card's single effect.
//
// Patched as a Prefix on CardEffect.GetFinalValue, mutating
// _EffectValueUpgraded directly (rather than the return value) so every
// downstream system reads the new number and no "modified" highlight is
// triggered. The card has only one effect, so no discriminator beyond the card
// ID is needed.
[HarmonyPatch(typeof(CardEffect), nameof(CardEffect.GetFinalValue))]
public static class InkwellAndQuillUpgradedValuePatch
{
    private const int InkwellAndQuillCardId = 1427;
    private const int UpgradedValue = 2;

    static void Prefix(CardEffect __instance)
    {
        var cardData = __instance.CardData;
        if (cardData == null || cardData._CardID != InkwellAndQuillCardId) return;

        __instance._EffectValueUpgraded = UpgradedValue;
    }
}
