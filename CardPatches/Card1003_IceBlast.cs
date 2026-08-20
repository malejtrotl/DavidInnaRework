using HarmonyLib;
using Rift;

namespace DavidInnaRework.CardPatches;

// Card 1003 "Ice Blast" has two effects: applying Frost, and dealing bonus
// damage if the target is already Frozen. We distinguish them via
// _Modifiers/_ConditionEffect (the "only if target has effect: Frozen" gate)
// vs _StatusType (Frost) on the plain status-applying effect.
//
// Here we also rewrite the condition itself: instead of "damage if target is
// Frozen", the damage effect now triggers "if target has no Shield" by
// swapping _Modifiers to EffectModifiers.OnlyIfTargetHasNoShield (this
// modifier is self-contained and doesn't need a companion _ConditionEffect
// value, unlike OnlyIfTargetHasEffect).
[HarmonyPatch(typeof(CardEffect), nameof(CardEffect.GetFinalValue))]
public static class IceBlastPatch
{
    private const int IceBlastCardId = 1003;
    internal const int FrostApplied = 5;
    internal const int DamageIfNoShield = 10;

    static void Prefix(CardEffect __instance)
    {
        var cardData = __instance.CardData;
        if (cardData == null || cardData._CardID != IceBlastCardId) return;

        bool isConditionalDamageEffect =
            (__instance._Modifiers == EffectModifiers.OnlyIfTargetHasEffect
                && __instance._ConditionEffect == AppliedEffectType.Frozen)
            || __instance._Modifiers == EffectModifiers.OnlyIfTargetHasNoShield;

        if (isConditionalDamageEffect)
        {
            // Rewrite the condition: "if frozen" -> "if no shield"
            __instance._Modifiers = EffectModifiers.OnlyIfTargetHasNoShield;
            __instance._ConditionEffect = AppliedEffectType.NONE;

            __instance._EffectValue = DamageIfNoShield;
            __instance._EffectValueUpgraded = DamageIfNoShield * 2;
        }
        else if (__instance._StatusType == StatusType.Frost)
        {
            __instance._EffectValue = FrostApplied;
            __instance._EffectValueUpgraded = FrostApplied * 2;
        }
    }
}

// Fixes up the tooltip text for Ice Blast so the condition wording matches
// the new "if it has no Shield" logic instead of the old "if it's Frozen"
// wording — WITHOUT touching how the numeric placeholders get filled in.
//
// _BaseDescription is the raw template string (with number placeholders)
// that GetDescription() reads and populates with the live/buffed values
// every time it's called. We patch this as a Prefix on GetDescription and
// edit _BaseDescription just before the original method runs, doing a
// targeted phrase replace (not a full rewrite) so the placeholder tokens for
// the numbers are left completely untouched — the game still fills them in
// itself afterwards, the exact same way it always did, buffs and all.
//
// We only ever replace the OLD phrase, so re-running this on every call is
// safe/idempotent: once the phrase has already been swapped, .Replace finds
// nothing left to change.
[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class IceBlastDescriptionPatch
{
    private const int IceBlastCardId = 1003;

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != IceBlastCardId) return;

        __instance._BaseDescription = "Apply {0} Frost to any enemy. Then, deal {1} damage to it if it has no shield.";
    }
}

// Renames Ice Blast (Card ID 1003) to "Piercing Ice".
//
// _Name is a plain writable field. We set it in a Prefix on
// CardData.GetDescription, which reliably fires whenever the game needs to
// display the card (both in combat and in the collection screen), so a
// single patch covers every place the name is shown.
[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class IceBlastNamePatch
{
    private const int IceBlastCardId = 1003;
    internal const string NewName = "Piercing Ice";

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != IceBlastCardId) return;

        __instance._Name = NewName;
    }
}
