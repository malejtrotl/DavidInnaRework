using HarmonyLib;
using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1400 "Improvise" reworked to:
//   - create only 1 tool (down from 2),
//   - then lose 1 (2 upgraded) mana,
//   - then, if you have no mana left, create 2 (3 upgraded) more tools.
//
// The original card only has the CreateTool effect, so the mana loss and the
// conditional second CreateTool are added as brand new CardEffects.
[HarmonyPatch(typeof(CardEffect), nameof(CardEffect.GetFinalValue))]
public static class ImproviseToolCountPatch
{
    private const int ImproviseCardId = 1400;
    private const int ToolsCreated = 1;

    static void Prefix(CardEffect __instance)
    {
        var cardData = __instance.CardData;
        if (cardData == null || cardData._CardID != ImproviseCardId) return;

        // Only touch the original, unconditional CreateTool effect — the mana
        // loss and the "if out of mana" CreateTool effect added by
        // ImproviseLoseManaThenCreateToolsPatch keep their own values.
        if (__instance._Mode != EffectMode.CreateTool) return;
        if (__instance._Modifiers == EffectModifiers.OnlyIfOutOfMana) return;

        __instance._EffectValue = ToolsCreated;
        __instance._EffectValueUpgraded = ToolsCreated;
    }
}

// Adds the two new effects, in play order after the existing CreateTool:
// negative AddMana
// CreateTool gated behind EffectModifiers.OnlyIfOutOfMana.
//
// Hooked on CardData.GetDescription because it fires once per card (unlike
// GetFinalValue, which fires once per effect), with guards making it
// idempotent since the Prefix runs on every call.
//
// Both new effects set CardData = __instance: effects loaded from the game's
// assets already have this owner back-reference, but `new CardEffect` does
// not, and a missing owner causes a NullReferenceException at runtime.
[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class ImproviseLoseManaThenCreateToolsPatch
{
    private const int ImproviseCardId = 1400;
    private const int ManaLost = 1;
    private const int ManaLostUpgraded = 2;
    private const int BonusTools = 2;
    private const int BonusToolsUpgraded = 3;
    private const int Cost = 1;

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != ImproviseCardId) return;

        __instance._Cost = Cost;
        __instance._CostUpgraded = Cost;

        bool hasManaLoss = false;
        bool hasConditionalTools = false;

        foreach (var existingEffect in __instance._Effects)
        {
            if (existingEffect._Mode == EffectMode.AddMana)
            {
                hasManaLoss = true;
            }
            else if (existingEffect._Mode == EffectMode.CreateTool
                && existingEffect._Modifiers == EffectModifiers.OnlyIfOutOfMana)
            {
                hasConditionalTools = true;
            }
        }

        if (hasManaLoss && hasConditionalTools) return;

        if (!hasManaLoss)
        {
            __instance._Effects.Add(new CardEffect
            {
                CardData = __instance,
                _Mode = EffectMode.AddMana,
                _Targeting = EffectTargeting.Self,
                _EffectValue = -ManaLost,
                _EffectValueUpgraded = -ManaLostUpgraded,
            });
        }

        if (!hasConditionalTools)
        {
            __instance._Effects.Add(new CardEffect
            {
                CardData = __instance,
                _Mode = EffectMode.CreateTool,
                _Modifiers = EffectModifiers.OnlyIfOutOfMana,
                _Targeting = EffectTargeting.Self,
                _EffectValue = BonusTools,
                _EffectValueUpgraded = BonusToolsUpgraded,
            });
        }
    }
}

[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class ImproviseDescriptionPatch
{
    private const int ImproviseCardId = 1400;
    private const string NewDescription =
        "Create {0} Tool. Lose {1} mana. If you have no mana, create {2} more Tools.";

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != ImproviseCardId) return;

        __instance._BaseDescription = NewDescription;
    }
}
