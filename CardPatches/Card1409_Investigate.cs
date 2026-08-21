using HarmonyLib;
using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1409 originally reads:
//   "Choose any enemy. Create 1 Tool, then 3 Tools if it has 3 or more debuff
//    types, then 3 Tools if it has 5 or more debuff types."
//
// The baseline (unconditional) CreateTool effect goes from 1 to 2 Tools, for
// both the unupgraded and upgraded versions. The two conditional CreateTool
// effects are left untouched.
//
// The card has THREE CreateTool effects, and GetFinalValue fires once per
// effect, so the Prefix must discriminate between them. The two conditional
// ones are identified by their _Modifiers gates; anything else is treated as
// the baseline effect. Discriminating by exclusion (rather than testing for
// EffectModifiers.NONE) avoids depending on a modifier value that the
// knowledge reference does not confirm exists.
[HarmonyPatch(typeof(CardEffect), nameof(CardEffect.GetFinalValue))]
public static class InvestigateToolCountPatch
{
    private const int TargetCardId = 1409;
    private const int BaselineTools = 2;

    static void Prefix(CardEffect __instance)
    {
        var cardData = __instance.CardData;
        if (cardData == null || cardData._CardID != TargetCardId) return;
        if (__instance._Mode != EffectMode.CreateTool) return;

        // Leave the two debuff-gated CreateTool effects alone.
        if (__instance._Modifiers == EffectModifiers.OnlyIfTargetHas3PlusDebuffs
            || __instance._Modifiers == EffectModifiers.OnlyIfTargetHas5PlusDebuffs)
        {
            return;
        }

        __instance._EffectValue = BaselineTools;
        __instance._EffectValueUpgraded = BaselineTools;
    }
}

[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class InvestigateDescriptionPatch
{
    private const int TargetCardId = 1409;
    private const string NewDescription =
        "Choose any enemy. Create {0} Tools, then {1} Tools if it has 3 or more debuff types, then {2} Tools if it has 5 or more debuff types.";

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != TargetCardId) return;

        __instance._BaseDescription = NewDescription;
    }
}
