using HarmonyLib;
using Rift;

namespace DavidInnaRework.CardPatches;

// Card with CardID 0052 ("Block"):
//   - Bumps its shield effect value from 12 to 50.
//   - Also grants 5 Tough to self when played (a brand new CardEffect, since
//     Block doesn't have a Tough-applying effect by default).
//   - Updated tooltip text (placeholder, wording TBD).
//
// The shield value fix is a Prefix on CardEffect.GetFinalValue (called
// whenever the game computes the actual value of a card effect, e.g. for
// display or applying the effect), which mutates the underlying
// _EffectValue field directly so every downstream system reads the new
// value directly (no "modified" indicator, e.g. green highlight).
//
// IMPORTANT: once the Tough effect exists on this card, GetFinalValue also
// fires for it (it's just another CardEffect on the same card) — so the
// Prefix must discriminate between the two effects (via _AppliedEffect) and
// only touch the shield effect's value, otherwise it would clobber the Tough
// effect's value too.
//
// Adding the Tough effect itself is done as a Prefix on
// CardData.GetDescription (fires once per card, unlike GetFinalValue which
// fires once per effect). A guard checks whether the effect was already
// added, making this idempotent since the Prefix runs on every call.
[HarmonyPatch(typeof(CardEffect), nameof(CardEffect.GetFinalValue))]
public static class ShieldCardBuffPatch
{
    private const int TargetCardId = 52;
    private const int NewValue = 50;

    static void Prefix(CardEffect __instance)
    {
        var cardData = __instance.CardData;
        if (cardData == null || cardData._CardID != TargetCardId) return;

        // Skip the Tough effect added by BlockGrantsToughPatch — only touch
        // the original shield effect.
        if (__instance._Mode == EffectMode.ApplyEffect
            && __instance._AppliedEffect == AppliedEffectType.Tough)
        {
            return;
        }

        __instance._EffectValue = NewValue;
        __instance._EffectValueUpgraded = NewValue * 2;
    }
}

// Makes Block (Card ID 52) also grant 5 Tough to self when played, in
// addition to its existing shield effect. Unlike patches that tweak an
// existing CardEffect's fields, this card doesn't have an effect that
// applies Tough at all, so instead we add a brand new CardEffect to the
// card's _Effects list.
//
// We hook this on CardData.GetDescription because it's called once per card
// (not once per effect like GetFinalValue), and reliably fires whenever the
// game needs to show/use the card. A guard flag (checking whether we already
// added our effect) makes this idempotent, since the Prefix runs on every
// call.
[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class BlockGrantsToughPatch
{
    private const int BlockCardId = 52;
    private const int ToughAmount = 5;

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != BlockCardId) return;

        // Idempotency guard: don't add the effect twice.
        foreach (var existingEffect in __instance._Effects)
        {
            if (existingEffect._Mode == EffectMode.ApplyEffect
                && existingEffect._AppliedEffect == AppliedEffectType.Tough)
            {
                return;
            }
        }

        var toughEffect = new CardEffect
        {
            _Mode = EffectMode.ApplyEffect,
            _AppliedEffect = AppliedEffectType.Tough,
            _Targeting = EffectTargeting.Self,
            _EffectValue = ToughAmount,
            _EffectValueUpgraded = ToughAmount * 2,
        };

        __instance._Effects.Add(toughEffect);
    }
}

// Placeholder for Block's (Card ID 52) updated tooltip text, to reflect the
// new "also grants Tough" effect added by BlockGrantsToughPatch. Same
// pattern as the Ice Blast description patch: a Prefix on
// CardData.GetDescription that overwrites _BaseDescription (the raw template
// with number placeholders) before the original method runs, so the game
// still fills in the placeholders itself afterwards.
//
// Left empty for now — fill in NewDescription with the real wording
// (including "{0}"/"{1}" placeholders as needed for the shield/Tough values)
// once decided.
[HarmonyPatch(typeof(CardData), nameof(CardData.GetDescription))]
public static class BlockDescriptionPatch
{
    private const int BlockCardId = 52;
    private const string NewDescription = "Gain {0} shield and Tough ({1}).";

    static void Prefix(CardData __instance)
    {
        if (__instance == null || __instance._CardID != BlockCardId) return;

        __instance._BaseDescription = NewDescription;
    }
}
