using HarmonyLib;
using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1411 "Resourceful Strike":
//   - Changes the damage from 3x2 to 2x3, or 2x5 upgraded.
//   - Changes the other effect from scaling per tool in hand to scaling per
//     tool played this turn.
//   - Converts that effect into temporary Powerful.
//
// The card has multiple effects, so each branch is discriminated explicitly.
[HarmonyPatch(typeof(CardEffect), nameof(CardEffect.GetFinalValue))]
public static class ResourcefulStrikeDamagePatch
{
    private const int ResourcefulStrikeCardId = 1411;
    private const int DamageValue = 2;
    private const int DamageValueUpgraded = 2;
    private const int DamageCount = 3;
    private const int DamageCountUpgraded = 5;

    static void Prefix(CardEffect __instance)
    {
        var cardData = __instance.CardData;
        if (cardData == null || cardData._CardID != ResourcefulStrikeCardId) return;
        if (__instance._Mode != EffectMode.Damage) return;

        __instance._EffectValue = DamageValue;
        __instance._EffectValueUpgraded = DamageValueUpgraded;
        __instance._EffectCount = DamageCount;
        __instance._EffectCountUpgraded = DamageCountUpgraded;
    }
}

[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class ResourcefulStrikeOtherEffectPatch
{
    private const int ResourcefulStrikeCardId = 1411;
    private const int PowerfulPerToolPlayed = 1;
    private const int PowerfulPerToolPlayedUpgraded = 1;
    private const AppliedEffectType ToolsPlayedThisTurnMarker = AppliedEffectType.COUNT;

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != ResourcefulStrikeCardId) return;

        foreach (var effect in __instance._Effects)
        {
            if (effect._Mode == EffectMode.Damage) continue;

            // Idempotency: this effect has already been converted.
            if (effect._Mode == EffectMode.ApplyEffectThisTurn
                && effect._AppliedEffect == AppliedEffectType.Powerful
                && effect._Modifiers == EffectModifiers.ScalePerStrikePlayed
                && effect._ConditionEffect == ToolsPlayedThisTurnMarker)
            {
                return;
            }

            if (effect._Modifiers != EffectModifiers.ScalePerTool) continue;

            effect._Mode = EffectMode.ApplyEffectThisTurn;
            effect._AppliedEffect = AppliedEffectType.Powerful;
            effect._Modifiers = EffectModifiers.ScalePerStrikePlayed;
            effect._ConditionEffect = ToolsPlayedThisTurnMarker;
            effect._EffectValue = PowerfulPerToolPlayed;
            effect._EffectValueUpgraded = PowerfulPerToolPlayedUpgraded;
        }
    }
}

[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class ResourcefulStrikeEffectOrderPatch
{
    private const int ResourcefulStrikeCardId = 1411;
    private const AppliedEffectType ToolsPlayedThisTurnMarker = AppliedEffectType.COUNT;

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != ResourcefulStrikeCardId) return;
        if (__instance._Effects.Count != 2) return;
        if (__instance._Effects[1]._ConditionEffect != ToolsPlayedThisTurnMarker) return;

        var firstEffect = __instance._Effects[0];
        var secondEffect = __instance._Effects[1];

        __instance._Effects.RemoveAt(1);
        __instance._Effects.RemoveAt(0);
        __instance._Effects.Add(secondEffect);
        __instance._Effects.Add(firstEffect);
    }
}

[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class ResourcefulStrikeDescriptionPatch
{
    private const int ResourcefulStrikeCardId = 1411;
    private const string NewDescription = "Gain Powerful ({0}) this turn per Tool played this turn. Deal {1} damage to the first enemy.";
    private const int Cost = 1;

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != ResourcefulStrikeCardId) return;

        __instance._BaseDescription = NewDescription;
        __instance._Cost = Cost;
        __instance._CostUpgraded = Cost;
    }
}
