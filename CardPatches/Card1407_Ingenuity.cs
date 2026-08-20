using HarmonyLib;
using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1407 "Ingenuity": new cost, different tool count, and completely new
// description text.
//
// All values below are PLACEHOLDERS — fill them in once decided.

// Changes how many tools Ingenuity creates.
//
// Patched as a Prefix on CardEffect.GetFinalValue, mutating _EffectValue
// directly (rather than the return value) so every downstream system reads the
// new number and no "modified" highlight is triggered.
//
// Guarded on _Mode so only the CreateTool effect is touched — the card may
// have other effects, and GetFinalValue fires once per effect.
[HarmonyPatch(typeof(CardEffect), nameof(CardEffect.GetFinalValue))]
public static class IngenuityToolCountPatch
{
    private const int IngenuityCardId = 1407;
    private const int ToolsCreated = 3;
    private const int ToolsCreatedUpgraded = 3;

    static void Prefix(CardEffect __instance)
    {
        var cardData = __instance.CardData;
        if (cardData == null || cardData._CardID != IngenuityCardId) return;
        if (__instance._Mode != EffectMode.CreateTool) return;

        __instance._EffectValue = ToolsCreated;
        __instance._EffectValueUpgraded = ToolsCreatedUpgraded;
    }
}

// Sets Ingenuity's mana cost and replaces its tooltip text outright.
//
// _Cost/_CostUpgraded and _BaseDescription are plain writable fields on
// CardData, set from a Prefix on GetDescription (fires once per card and
// reliably whenever the card needs displaying). _BaseDescription is the raw
// template — keep the {0}/{1}... placeholders in the new wording so the game
// still fills in the live/buffed numbers itself.
[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class IngenuityCostAndDescriptionPatch
{
    private const int IngenuityCardId = 1407;

    // PLACEHOLDER: mana cost, unupgraded / upgraded.
    private const int Cost = 4;
    private const int CostUpgraded = 2;

    // PLACEHOLDER: full replacement wording.
    private const string NewDescription = "Create {0} Tools, then upgrade all Tool cards in hand.";

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != IngenuityCardId) return;

        __instance._Cost = Cost;
        __instance._CostUpgraded = CostUpgraded;
        __instance._BaseDescription = NewDescription;
    }
}
